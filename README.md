# KYJ Reminder WOW

World of Warcraft 보스 레이드를 위한 타임라인 기반 리마인더 애플리케이션

## 📋 프로젝트 소개

WoW 게임 외부에서 실행되는 독립형 프로그램으로, 보스 전투 타임라인에 따라 특정 시간에 알림을 표시합니다.
Liquid Reminder 애드온에서 영감을 받아 제작되었습니다.

## ✨ 주요 기능

- **타임라인 기반 리마인더**: 3분 타임라인에 자유롭게 리마인더 추가
- **리마인더 설정**: 타임라인 중 원하는 부분에 더블 후 알림 받을 내용 작성
- **토스트 알림**: 표기한 리마인더 시간이 되었을 때, 우측 하단 토스트 알림
- **재생 컨트롤**: Play/Pause/Reset 기능

## 🖼️ 미리보기

<!-- 여기에 UI 이미지 추가 -->
![메인 화면](./screenshots/main.png)
![타임라인](./screenshots/timeline.png)
![알림 예시](./screenshots/notification.png)

## 🛠️ 기술 스택

- **Framework**: .NET Framework 4.5.2
- **UI**: WPF (Windows Presentation Foundation)

## 📦 설치 방법

### 실행 파일 다운로드

1. [Releases](https://github.com/yoyoyoun18/KYJReminderWOW/releases) 페이지에서 최신 버전 다운로드
2. `KimYoungJoReminder.exe` 실행

### 소스 코드 빌드

```bash
# 저장소 클론
git clone https://github.com/yoyoyoun18/KYJReminderWOW.git

# Visual Studio로 솔루션 열기
KimYoungJoReminder.sln

# F5 또는 Ctrl+F5로 빌드 및 실행
```

## 🚀 실행 방법

### 실행 파일
```
KimYoungJoReminder\bin\Debug\KimYoungJoReminder.exe
```

### Visual Studio
1. 솔루션 열기 (`KimYoungJoReminder.sln`)
2. F5 키 또는 "디버그 시작" 클릭
3. 또는 Ctrl+F5 (디버깅 없이 실행)

## 📖 사용 방법

### 1. 리마인더 추가
1. 타임라인 위에서 원하는 시간 지점을 **더블클릭**
2. 입력창에 알림 메시지 작성
3. OK 버튼 클릭

### 2. 타임라인 재생
1. **Play** 버튼 클릭으로 시작
2. **Pause** 버튼으로 일시정지 (이어서 재생 가능)
3. **Reset** 버튼으로 초기화

### 3. 알림 확인
- 지정한 시간에 도달하면 우측 하단에 토스트 알림 표시
- 알림은 5초 후 자동으로 사라짐
- 알림 클릭 시 즉시 닫힘

## 🗺️ 업데이트 예정 사항
- [ ] 리마인더 우클릭 메뉴 (수정/삭제)
- [ ] JSON 기반 프로필 저장/불러오기
- [ ] 보스별 프로필 관리
- [ ] 타임라인 길이 사용자 설정
- [ ] 재생 속도 조절 (0.5x, 1x, 2x)
- [ ] 여러 토스트 알림 동시 표시 시 쌓기
- [ ] Play, Pause, Reset 버튼 키보트 버튼 매핑

