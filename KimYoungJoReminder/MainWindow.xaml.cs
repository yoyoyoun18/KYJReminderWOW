using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using KimYoungJoReminder.Models;
using KimYoungJoReminder.Views;

namespace KimYoungJoReminder
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 1초당 픽셀 수 (타임라인 스케일)
        /// </summary>
        private const int PIXELS_PER_SECOND = 10;

        /// <summary>
        /// 타임라인 관리자
        /// </summary>
        private TimelineManager _timelineManager;

        /// <summary>
        /// 현재 재생 위치를 나타내는 라인
        /// </summary>
        private Line _playheadLine;

        /// <summary>
        /// 리마인더 마커 딕셔너리 (시간 -> Rectangle)
        /// </summary>
        private Dictionary<int, Rectangle> _reminderMarkers;

        /// <summary>
        /// 드래그 리사이징 상태
        /// </summary>
        private bool _isResizing = false;
        private Rectangle _resizingMarker = null;
        private double _resizeStartX = 0;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        private void Initialize()
        {
            _timelineManager = new TimelineManager();
            _reminderMarkers = new Dictionary<int, Rectangle>();

            // TimelineManager 이벤트 구독
            _timelineManager.TimeUpdated += OnTimeUpdated;
            _timelineManager.ReminderTriggered += OnReminderTriggered;
            _timelineManager.StateChanged += OnStateChanged;

            // 타임라인 초기화
            DrawTimelineGrid();
            CreatePlayhead();
        }

        /// <summary>
        /// 타임라인 눈금 그리기
        /// </summary>
        private void DrawTimelineGrid()
        {
            // 10초마다 세로선 및 시간 텍스트 표시
            for (int second = 0; second <= TimelineManager.MAX_TIMELINE_SECONDS; second += 10)
            {
                double x = second * PIXELS_PER_SECOND;

                // 세로선
                Line gridLine = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = timelineCanvas.Height,
                    Stroke = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                    StrokeThickness = 1
                };
                timelineCanvas.Children.Add(gridLine);

                // 시간 텍스트
                int minutes = second / 60;
                int seconds = second % 60;
                string timeText = string.Format("{0:D2}:{1:D2}", minutes, seconds);

                TextBlock timeLabel = new TextBlock
                {
                    Text = timeText,
                    Foreground = new SolidColorBrush(Colors.Gray),
                    FontSize = 10
                };
                Canvas.SetLeft(timeLabel, x + 2);
                Canvas.SetTop(timeLabel, 5);
                timelineCanvas.Children.Add(timeLabel);
            }
        }

        /// <summary>
        /// 재생 헤드(현재 위치 표시선) 생성
        /// </summary>
        private void CreatePlayhead()
        {
            _playheadLine = new Line
            {
                X1 = 0,
                Y1 = 0,
                X2 = 0,
                Y2 = timelineCanvas.Height,
                Stroke = new SolidColorBrush(Color.FromRgb(78, 201, 176)), // 청록색
                StrokeThickness = 2
            };
            timelineCanvas.Children.Add(_playheadLine);
        }

        /// <summary>
        /// 재생 헤드 위치 업데이트
        /// </summary>
        private void UpdatePlayheadPosition()
        {
            // Stopwatch 기반 실시간 위치 계산
            double totalSeconds = _timelineManager.GetCurrentTimeInSecondsWithDecimal();
            double targetX = totalSeconds * PIXELS_PER_SECOND;

            // 애니메이션 취소하고 직접 위치 설정 (부드러운 업데이트는 0.1초 간격으로 자동)
            _playheadLine.BeginAnimation(Line.X1Property, null);
            _playheadLine.BeginAnimation(Line.X2Property, null);
            _playheadLine.X1 = targetX;
            _playheadLine.X2 = targetX;
        }

        /// <summary>
        /// 타임라인 Canvas 더블클릭 이벤트 (현재는 단일 클릭으로 처리)
        /// </summary>
        private void TimelineCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 재생 중에는 클릭 방지
            if (_timelineManager.State == TimelineState.Playing)
                return;

            // 더블클릭 체크
            if (e.ClickCount == 2)
            {
                Point clickPosition = e.GetPosition(timelineCanvas);
                int clickedSecond = (int)(clickPosition.X / PIXELS_PER_SECOND);

                // 범위 체크
                if (clickedSecond < 0 || clickedSecond >= TimelineManager.MAX_TIMELINE_SECONDS)
                    return;

                // 리마인더 입력 창 표시
                ShowReminderInputDialog(clickedSecond);
            }
        }

        /// <summary>
        /// 리마인더 입력 다이얼로그 표시
        /// </summary>
        private void ShowReminderInputDialog(int timeInSeconds)
        {
            // 간단한 InputBox 구현
            var inputWindow = new Window
            {
                Title = "Add Reminder",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 45))
            };

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // 시간 표시
            var timeLabel = new TextBlock
            {
                Text = $"Start Time: {FormatTime(timeInSeconds)}",
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(timeLabel, 0);
            grid.Children.Add(timeLabel);

            // 텍스트 입력
            var textBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(5)
            };
            Grid.SetRow(textBox, 1);
            grid.Children.Add(textBox);

            // Duration 입력
            var durationPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            var durationLabel = new TextBlock
            {
                Text = "Duration (seconds):",
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            var durationTextBox = new TextBox
            {
                Text = "5",
                Width = 60,
                Padding = new Thickness(5)
            };
            durationPanel.Children.Add(durationLabel);
            durationPanel.Children.Add(durationTextBox);
            Grid.SetRow(durationPanel, 2);
            grid.Children.Add(durationPanel);

            // 버튼 패널
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0)
            };
            okButton.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    int duration = 5;
                    int.TryParse(durationTextBox.Text, out duration);
                    if (duration < 1) duration = 1;

                    if (_timelineManager.AddReminder(timeInSeconds, textBox.Text, duration))
                    {
                        AddReminderMarker(timeInSeconds, textBox.Text, duration);
                        inputWindow.DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show("A reminder already exists at this time.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 80,
                Height = 30
            };
            cancelButton.Click += (s, e) => { inputWindow.DialogResult = false; };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 3);
            grid.Children.Add(buttonPanel);

            inputWindow.Content = grid;
            inputWindow.ShowDialog();
        }

        /// <summary>
        /// 타임라인에 리마인더 마커 추가
        /// </summary>
        private void AddReminderMarker(int timeInSeconds, string text, int duration)
        {
            double x = timeInSeconds * PIXELS_PER_SECOND;
            double width = duration * PIXELS_PER_SECOND;

            // 마커 사각형
            Rectangle marker = new Rectangle
            {
                Width = width,
                Height = timelineCanvas.Height - 30,
                Fill = new SolidColorBrush(Color.FromRgb(255, 165, 0)), // 주황색
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1,
                Tag = timeInSeconds, // 시작 시간 정보 저장
                Cursor = Cursors.Hand
            };

            Canvas.SetLeft(marker, x);
            Canvas.SetTop(marker, 30);
            timelineCanvas.Children.Add(marker);

            // 툴팁 추가 (시작~종료 시간 표시)
            int endTime = timeInSeconds + duration;
            marker.ToolTip = $"{FormatTime(timeInSeconds)} ~ {FormatTime(endTime)}: {text}";

            // 컨텍스트 메뉴 생성
            ContextMenu contextMenu = new ContextMenu();

            MenuItem editMenuItem = new MenuItem { Header = "수정" };
            editMenuItem.Click += (s, e) => EditReminder(timeInSeconds);

            MenuItem deleteMenuItem = new MenuItem { Header = "삭제" };
            deleteMenuItem.Click += (s, e) => DeleteReminder(timeInSeconds);

            contextMenu.Items.Add(editMenuItem);
            contextMenu.Items.Add(deleteMenuItem);
            marker.ContextMenu = contextMenu;

            // 마우스 이벤트 추가 (드래그 리사이징)
            marker.MouseDown += Marker_MouseDown;
            marker.MouseMove += Marker_MouseMove;
            marker.MouseUp += Marker_MouseUp;
            marker.MouseEnter += Marker_MouseEnter;
            marker.MouseLeave += Marker_MouseLeave;

            _reminderMarkers[timeInSeconds] = marker;
        }

        /// <summary>
        /// 리마인더 삭제
        /// </summary>
        private void DeleteReminder(int timeInSeconds)
        {
            // TimelineManager에서 삭제
            if (_timelineManager.RemoveReminder(timeInSeconds))
            {
                // UI에서 마커 제거
                if (_reminderMarkers.ContainsKey(timeInSeconds))
                {
                    Rectangle marker = _reminderMarkers[timeInSeconds];
                    timelineCanvas.Children.Remove(marker);
                    _reminderMarkers.Remove(timeInSeconds);
                }
            }
        }

        /// <summary>
        /// 리마인더 수정
        /// </summary>
        private void EditReminder(int timeInSeconds)
        {
            // 기존 리마인더 정보 가져오기
            var existingReminder = _timelineManager.GetAllReminders()
                .FirstOrDefault(r => r.TimeInSeconds == timeInSeconds);

            if (existingReminder == null)
                return;

            // 입력 다이얼로그 표시
            var inputWindow = new Window
            {
                Title = "Edit Reminder",
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 45))
            };

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // 시간 표시
            var timeLabel = new TextBlock
            {
                Text = $"Time: {FormatTime(timeInSeconds)}",
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(timeLabel, 0);
            grid.Children.Add(timeLabel);

            // 텍스트 입력 (기존 텍스트로 초기화)
            var textBox = new TextBox
            {
                Text = existingReminder.Text,
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(5)
            };
            Grid.SetRow(textBox, 1);
            grid.Children.Add(textBox);

            // 버튼 패널
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0)
            };
            okButton.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    // 기존 텍스트 업데이트
                    existingReminder.Text = textBox.Text;

                    // UI 마커의 툴팁 업데이트
                    if (_reminderMarkers.ContainsKey(timeInSeconds))
                    {
                        _reminderMarkers[timeInSeconds].ToolTip = $"{FormatTime(timeInSeconds)}: {textBox.Text}";
                    }

                    inputWindow.DialogResult = true;
                }
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 80,
                Height = 30
            };
            cancelButton.Click += (s, e) => { inputWindow.DialogResult = false; };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            inputWindow.Content = grid;
            inputWindow.ShowDialog();
        }

        /// <summary>
        /// 시간 포맷 (MM:SS)
        /// </summary>
        private string FormatTime(int seconds)
        {
            int minutes = seconds / 60;
            int secs = seconds % 60;
            return string.Format("{0:D2}:{1:D2}", minutes, secs);
        }

        #region Event Handlers

        /// <summary>
        /// 시간 업데이트 이벤트 핸들러
        /// </summary>
        private void OnTimeUpdated(object sender, int timeInSeconds)
        {
            txtCurrentTime.Text = _timelineManager.GetFormattedCurrentTimeWithDecimal();
            UpdatePlayheadPosition();
        }

        /// <summary>
        /// 리마인더 트리거 이벤트 핸들러
        /// </summary>
        private void OnReminderTriggered(object sender, ReminderItem reminder)
        {
            // 토스트 알림 표시
            ToastNotification.Show(
                $"⏰ Reminder at {reminder.GetFormattedTime()}",
                reminder.Text,
                5 // 5초 동안 표시
            );
        }

        /// <summary>
        /// 상태 변경 이벤트 핸들러
        /// </summary>
        private void OnStateChanged(object sender, TimelineState state)
        {
            switch (state)
            {
                case TimelineState.Playing:
                    btnPlay.IsEnabled = false;
                    btnPause.IsEnabled = true;
                    break;

                case TimelineState.Paused:
                case TimelineState.Stopped:
                    btnPlay.IsEnabled = true;
                    btnPause.IsEnabled = false;
                    break;

                case TimelineState.Completed:
                    btnPlay.IsEnabled = false;
                    btnPause.IsEnabled = false;
                    MessageBox.Show("Timeline completed!", "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }

        /// <summary>
        /// Play 버튼 클릭
        /// </summary>
        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            _timelineManager.Play();
        }

        /// <summary>
        /// Pause 버튼 클릭
        /// </summary>
        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            _timelineManager.Pause();
        }

        /// <summary>
        /// Reset 버튼 클릭
        /// </summary>
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            _timelineManager.Reset();
        }

        /// <summary>
        /// 마커 마우스 엔터 - 리사이징 영역이면 커서 변경
        /// </summary>
        private void Marker_MouseEnter(object sender, MouseEventArgs e)
        {
            var marker = sender as Rectangle;
            if (marker != null)
            {
                Point pos = e.GetPosition(marker);
                if (pos.X > marker.Width - 5) // 오른쪽 끝 5px 이내
                {
                    marker.Cursor = Cursors.SizeWE;
                }
                else
                {
                    marker.Cursor = Cursors.Hand;
                }
            }
        }

        /// <summary>
        /// 마커 마우스 리브 - 커서 초기화
        /// </summary>
        private void Marker_MouseLeave(object sender, MouseEventArgs e)
        {
            var marker = sender as Rectangle;
            if (!_isResizing && marker != null)
            {
                marker.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// 마커 마우스 다운 - 드래그 시작
        /// </summary>
        private void Marker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var marker = sender as Rectangle;
            if (e.LeftButton == MouseButtonState.Pressed && marker != null)
            {
                Point pos = e.GetPosition(marker);
                if (pos.X > marker.Width - 5) // 오른쪽 끝 리사이징
                {
                    _isResizing = true;
                    _resizingMarker = marker;
                    _resizeStartX = e.GetPosition(timelineCanvas).X;
                    marker.CaptureMouse();
                    e.Handled = true; // 컨텍스트 메뉴 방지
                }
            }
        }

        /// <summary>
        /// 마커 마우스 이동 - 리사이징 중 크기 조절
        /// </summary>
        private void Marker_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isResizing && _resizingMarker != null)
            {
                double currentX = e.GetPosition(timelineCanvas).X;
                double markerLeft = Canvas.GetLeft(_resizingMarker);
                double newWidth = currentX - markerLeft;

                // 최소 너비 (1초 = 10px)
                if (newWidth >= PIXELS_PER_SECOND)
                {
                    _resizingMarker.Width = newWidth;

                    // 툴팁 업데이트
                    int timeInSeconds = (int)_resizingMarker.Tag;
                    int duration = (int)(newWidth / PIXELS_PER_SECOND);
                    var reminder = _timelineManager.GetAllReminders()
                        .FirstOrDefault(r => r.TimeInSeconds == timeInSeconds);
                    if (reminder != null)
                    {
                        int endTime = timeInSeconds + duration;
                        _resizingMarker.ToolTip = $"{FormatTime(timeInSeconds)} ~ {FormatTime(endTime)}: {reminder.Text}";
                    }
                }
            }
            else
            {
                var marker = sender as Rectangle;
                if (marker != null)
                {
                    // 커서 변경 (리사이징 영역 체크)
                    Point pos = e.GetPosition(marker);
                    if (pos.X > marker.Width - 5)
                    {
                        marker.Cursor = Cursors.SizeWE;
                    }
                    else
                    {
                        marker.Cursor = Cursors.Hand;
                    }
                }
            }
        }

        /// <summary>
        /// 마커 마우스 업 - 드래그 종료 및 데이터 동기화
        /// </summary>
        private void Marker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizing && _resizingMarker != null)
            {
                // TimelineManager 데이터 업데이트
                int timeInSeconds = (int)_resizingMarker.Tag;
                int newDuration = (int)(_resizingMarker.Width / PIXELS_PER_SECOND);

                var reminder = _timelineManager.GetAllReminders()
                    .FirstOrDefault(r => r.TimeInSeconds == timeInSeconds);
                if (reminder != null)
                {
                    reminder.Duration = newDuration;
                }

                _resizingMarker.ReleaseMouseCapture();
                _resizingMarker = null;
                _isResizing = false;
            }
        }

        #endregion
    }
}
