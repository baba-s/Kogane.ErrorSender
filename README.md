# UniErrorSender

サーバにエラーログを送信する時に不必要な情報は送信しないようにできる機能

## 特徴

* 同じエラーは連続して送信しない  
* 一定時間内に連続してエラーが発生した場合、それらのエラーは送信しない  
* スタックトレースから不要な行は除外可能  
* スタックトレースの行数を制限可能  

## 使用例

```cs
using Kogane;
using UnityEngine;

public class Example : MonoBehaviour
{
    private sealed class ErrorSenderHandle : IErrorSenderHandle
    {
        public void Send( string condition, string stackTrace, LogType type )
        {
            // ここにサーバにエラーログを送信する処理を記述
        }
    }

    // サーバにエラーを送信するためのインスタンス
    private readonly ErrorSender m_errorSender = new ErrorSender( new ErrorSenderHandle() )
    {
        // 連続でエラーを送信する時の待機時間
        // この待機時間の間に発生したエラーは送信しない
        Interval = 0.5f,

        // スタックトレースから除外したい文字列のリスト
        // StartsWith で一致したスタックトレースの行は除外
        IgnoreStackTraceList = new string[0],

        // 送信するスタックトレースの行数
        StackTraceLines = 10,
    };

    private void Awake()
    {
        // ログ出力された時に呼び出されるイベントにコールバックを登録
        Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
    }

    private void OnLogMessageReceivedThreaded( string condition, string stacktrace, LogType type )
    {
        // エラーや例外ではない場合は無視
        if ( type != LogType.Error && type != LogType.Assert && type != LogType.Exception ) return;

        // エラーや例外はログの情報をサーバに送信
        m_errorSender.Send( condition, stacktrace, type );
    }
}
```
