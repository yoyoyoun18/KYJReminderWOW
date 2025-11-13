namespace KimYoungJoReminder.Models
{
    /// <summary>
    /// 타임라인 재생 상태
    /// </summary>
    public enum TimelineState
    {
        /// <summary>
        /// 정지 상태 (초기 상태 또는 리셋 후)
        /// </summary>
        Stopped,

        /// <summary>
        /// 재생 중
        /// </summary>
        Playing,

        /// <summary>
        /// 일시정지
        /// </summary>
        Paused,

        /// <summary>
        /// 완료 (3분 도달)
        /// </summary>
        Completed
    }
}
