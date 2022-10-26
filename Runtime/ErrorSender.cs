using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kogane
{
    /// <summary>
    /// エラーを送信するインターフェイス
    /// </summary>
    public interface IErrorSenderHandle
    {
        //==============================================================================
        // 関数
        //==============================================================================
        /// <summary>
        /// エラーを送信する時に呼び出される関数
        /// </summary>
        void Send( string condition, string stackTrace, LogType type );
    }

    /// <summary>
    /// エラーを送信するクラス
    /// </summary>
    public sealed class ErrorSender
    {
        //==============================================================================
        // 変数(readonly)
        //==============================================================================
        private readonly IErrorSenderHandle m_handle;

        //==============================================================================
        // プロパティ
        //==============================================================================
        /// <summary>
        /// 連続でエラーを送信する時の待機時間
        /// この待機時間の間に送信しようとしたエラーは無視します
        /// </summary>
        public float Interval { private get; set; }

        /// <summary>
        /// スタックトレースから除外したい文字列のリスト
        /// StartsWith で一致したスタックトレースの行は無視します
        /// </summary>
        public IReadOnlyList<string> IgnoreStackTraceList { private get; set; } = new string[ 0 ];

        /// <summary>
        /// 送信するスタックトレースの行数
        /// </summary>
        public int StackTraceLines { private get; set; } = 10;

        //==============================================================================
        // 変数
        //==============================================================================
        private string m_lastCondition;
        private string m_lastStackTrace;
        private float  m_lastTime;

        //==============================================================================
        // 関数
        //==============================================================================
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ErrorSender( IErrorSenderHandle handle )
        {
            m_handle = handle;
        }

        /// <summary>
        /// 最後に送信したエラーの情報を破棄します
        /// </summary>
        public void Clear()
        {
            m_lastCondition  = default;
            m_lastStackTrace = default;
            m_lastTime       = default;
        }

        /// <summary>
        /// エラーを送信します
        /// </summary>
        public void Send( string condition, string stackTrace, LogType type )
        {
            // Unity を再生していない時は送信しません
            if ( !Application.isPlaying ) return;

#if UNITY_EDITOR
            // コンパイル中は送信しません
            if ( UnityEditor.EditorApplication.isCompiling ) return;
#endif

            // 最後に送信したエラーと同じ場合は送信しません
            if ( m_lastCondition == condition && m_lastStackTrace == stackTrace ) return;

            // 連続でエラーを送信しようとした場合は無視します
            var realtimeSinceStartup = Time.realtimeSinceStartup;
            if ( Mathf.Abs( realtimeSinceStartup - m_lastTime ) < Interval ) return;

            // 最後に送信したエラーの情報を記憶しておきます
            m_lastCondition  = condition;
            m_lastStackTrace = stackTrace;
            m_lastTime       = Time.realtimeSinceStartup;

            // スタックトレースから除外したい行を除外し、指定の行数に抑えます
            var stackTraceExcludingIgnores = stackTrace
                    .Split( '\n' )
                    .Where( x => IgnoreStackTraceList.All( y => !x.StartsWith( y ) ) )
                    .Take( StackTraceLines )
                ;

            stackTrace = string.Join( "\n", stackTraceExcludingIgnores ).TrimEnd();

            // 送信します
            m_handle.Send( condition, stackTrace, type );
        }
    }
}