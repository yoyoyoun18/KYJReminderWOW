using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using KimYoungJoReminder.Models;

namespace KimYoungJoReminder.Components
{
    /// <summary>
    /// 타임라인에 표시되는 리마인더 마커 UI 컴포넌트
    /// (React의 컴포넌트와 유사한 역할)
    /// </summary>
    public class ReminderMarker
    {
        #region Fields

        /// <summary>
        /// 리마인더 데이터 (Model)
        /// </summary>
        public ReminderItem Data { get; private set; }

        /// <summary>
        /// 마커 Rectangle UI
        /// </summary>
        public Rectangle MarkerUI { get; private set; }

        /// <summary>
        /// 부모 Canvas
        /// </summary>
        private Canvas _canvas;

        /// <summary>
        /// TimelineManager 참조 (데이터 관리용)
        /// </summary>
        private TimelineManager _timelineManager;

        /// <summary>
        /// 1초당 픽셀 수
        /// </summary>
        private double _pixelsPerSecond;

        /// <summary>
        /// 리사이징 중 여부
        /// </summary>
        private bool _isResizing = false;

        /// <summary>
        /// 리사이징 핸들 너비 (오른쪽 끝 감지 영역)
        /// </summary>
        private const double RESIZE_HANDLE_WIDTH = 5;

        #endregion

        #region Constructor

        /// <summary>
        /// ReminderMarker 생성자
        /// </summary>
        /// <param name="data">리마인더 데이터</param>
        /// <param name="canvas">부모 Canvas</param>
        /// <param name="timelineManager">TimelineManager 인스턴스</param>
        /// <param name="pixelsPerSecond">1초당 픽셀 수</param>
        public ReminderMarker(ReminderItem data, Canvas canvas, TimelineManager timelineManager, double pixelsPerSecond)
        {
            Data = data;
            _canvas = canvas;
            _timelineManager = timelineManager;
            _pixelsPerSecond = pixelsPerSecond;

            CreateUI();
            AttachEventHandlers();
        }

        #endregion

        #region UI Creation

        /// <summary>
        /// Rectangle UI 생성 및 Canvas에 추가
        /// </summary>
        private void CreateUI()
        {
            double x = Data.TimeInSeconds * _pixelsPerSecond;
            double width = Data.Duration * _pixelsPerSecond;

            MarkerUI = new Rectangle
            {
                Width = width,
                Height = _canvas.Height - 180,
                Fill = new SolidColorBrush(Color.FromRgb(255, 165, 0)), // 주황색
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1,
                Tag = Data.TimeInSeconds, // 시작 시간 저장
                Cursor = Cursors.Hand
            };

            Canvas.SetLeft(MarkerUI, x);
            Canvas.SetTop(MarkerUI, 30);

            UpdateTooltip();
            CreateContextMenu();

            _canvas.Children.Add(MarkerUI);
        }

        /// <summary>
        /// 툴팁 업데이트
        /// </summary>
        private void UpdateTooltip()
        {
            int endTime = Data.GetEndTimeInSeconds();
            MarkerUI.ToolTip = $"{Data.GetFormattedTime()} ~ {Data.GetFormattedEndTime()}: {Data.Text}";
        }

        /// <summary>
        /// 우클릭 컨텍스트 메뉴 생성
        /// </summary>
        private void CreateContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu();

            // 수정 메뉴
            MenuItem editMenuItem = new MenuItem { Header = "Edit" };
            editMenuItem.Click += OnEditClicked;
            contextMenu.Items.Add(editMenuItem);

            // 삭제 메뉴
            MenuItem deleteMenuItem = new MenuItem { Header = "Delete" };
            deleteMenuItem.Click += OnDeleteClicked;
            contextMenu.Items.Add(deleteMenuItem);

            MarkerUI.ContextMenu = contextMenu;
        }

        #endregion

        #region Event Handlers - Context Menu

        /// <summary>
        /// 수정 메뉴 클릭 이벤트
        /// </summary>
        private void OnEditClicked(object sender, RoutedEventArgs e)
        {
            // MainWindow에서 처리하도록 이벤트 발생
            EditRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 삭제 메뉴 클릭 이벤트
        /// </summary>
        private void OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            // MainWindow에서 처리하도록 이벤트 발생
            DeleteRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Event Handlers - Mouse Drag/Resize

        /// <summary>
        /// 마우스 이벤트 핸들러 연결
        /// </summary>
        private void AttachEventHandlers()
        {
            MarkerUI.MouseDown += OnMouseDown;
            MarkerUI.MouseMove += OnMouseMove;
            MarkerUI.MouseUp += OnMouseUp;
            MarkerUI.MouseEnter += OnMouseEnter;
            MarkerUI.MouseLeave += OnMouseLeave;
        }

        /// <summary>
        /// 마우스 엔터 - 리사이징 영역이면 커서 변경
        /// </summary>
        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            Point pos = e.GetPosition(MarkerUI);
            if (pos.X > MarkerUI.Width - RESIZE_HANDLE_WIDTH)
            {
                MarkerUI.Cursor = Cursors.SizeWE;
            }
            else
            {
                MarkerUI.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// 마우스 리브 - 커서 초기화
        /// </summary>
        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!_isResizing)
            {
                MarkerUI.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// 마우스 다운 - 드래그 시작
        /// </summary>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point pos = e.GetPosition(MarkerUI);
                if (pos.X > MarkerUI.Width - RESIZE_HANDLE_WIDTH) // 오른쪽 끝 리사이징
                {
                    _isResizing = true;
                    MarkerUI.CaptureMouse();
                    e.Handled = true; // 컨텍스트 메뉴 방지
                }
            }
        }

        /// <summary>
        /// 마우스 이동 - 리사이징 중 크기 조절
        /// </summary>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isResizing)
            {
                // UI 계산 (픽셀 단위)
                double currentX = e.GetPosition(_canvas).X;
                double markerLeft = Canvas.GetLeft(MarkerUI);
                double newWidth = currentX - markerLeft;

                // 최소 너비 (1초)
                if (newWidth >= _pixelsPerSecond)
                {
                    // UI 업데이트
                    MarkerUI.Width = newWidth;

                    // 툴팁 실시간 업데이트 (비즈니스 데이터로 변환)
                    int newDuration = (int)(newWidth / _pixelsPerSecond);
                    int endTime = Data.TimeInSeconds + newDuration;
                    MarkerUI.ToolTip = $"{Data.GetFormattedTime()} ~ {FormatTime(endTime)}: {Data.Text}";
                }
            }
            else
            {
                // 커서 변경 (리사이징 영역 체크)
                Point pos = e.GetPosition(MarkerUI);
                if (pos.X > MarkerUI.Width - RESIZE_HANDLE_WIDTH)
                {
                    MarkerUI.Cursor = Cursors.SizeWE;
                }
                else
                {
                    MarkerUI.Cursor = Cursors.Hand;
                }
            }
        }

        /// <summary>
        /// 마우스 업 - 드래그 종료 및 데이터 동기화
        /// </summary>
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizing)
            {
                // UI → 비즈니스 데이터 변환
                int newDuration = (int)(MarkerUI.Width / _pixelsPerSecond);

                // 비즈니스 로직 호출 (Model에서 처리)
                Data.Duration = newDuration;

                // 최종 툴팁 업데이트
                UpdateTooltip();

                MarkerUI.ReleaseMouseCapture();
                _isResizing = false;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Canvas에서 마커 제거
        /// </summary>
        public void Remove()
        {
            _canvas.Children.Remove(MarkerUI);
        }

        /// <summary>
        /// 마커 위치/크기 업데이트 (외부에서 데이터 변경 시)
        /// </summary>
        public void UpdateUI()
        {
            double x = Data.TimeInSeconds * _pixelsPerSecond;
            double width = Data.Duration * _pixelsPerSecond;

            Canvas.SetLeft(MarkerUI, x);
            MarkerUI.Width = width;
            UpdateTooltip();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 시간을 MM:SS 형식으로 포맷
        /// </summary>
        private string FormatTime(int timeInSeconds)
        {
            int minutes = timeInSeconds / 60;
            int seconds = timeInSeconds % 60;
            return string.Format("{0:D2}:{1:D2}", minutes, seconds);
        }

        #endregion

        #region Events

        /// <summary>
        /// 수정 요청 이벤트 (MainWindow에서 처리)
        /// </summary>
        public event EventHandler EditRequested;

        /// <summary>
        /// 삭제 요청 이벤트 (MainWindow에서 처리)
        /// </summary>
        public event EventHandler DeleteRequested;

        #endregion
    }
}
