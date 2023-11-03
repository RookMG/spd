# 단축키

|    단축키    |     기능      |        비고         |
| :----------: | :-----------: | :-----------------: |
|     `F4`     | 속성 창 표시  |          -          |
| `Shift`+`F7` | 디자이너 표시 | window.xaml 선택 후 |

<br>

# 기초

## 솔루션 탐색기

솔루션 탐색기에서 현재 솔루션(프로젝트들을 담는 하나의 큰 프로젝트)에 등록된 프로젝트에 대한 정보와 구조들을 볼 수 있음

### 솔루션 탐색기 파일 구성

```
xaml 이란?
- Extensible Application Markup Language
- MS에서 개발한 XML 기반의 언어로, WPF, Xamarin, UNO에서 사용하는  UI 인터페이스 객체(Window, Button, Grid, ...)을 XML 형태로 정의함
- 웹 페이지에서 HTML을 사용하여 시각적 표현을 사용하는 것과 비슷
- XML 태그를 사용하여 디자이너가 직접 UI를 구성, 로직을 담당하는 부분과 분리 가능
- 필수로 필요한 것은 아님, C#만 사용하여 개발 가능
- XAML을 사용하여 애니메이션 구현 가능. 이벤트 트리거를 추가 구현하여 응답하도록 구성 가능
- WPF는 벡터 방식 이미지 지원
```

**App.xaml**<br>
현재 프로젝트의 시작점이 되는 xaml파일.<br>
리소스들을 미리 선언하여 관리하는 곳.<br>
App.xaml.cs 파일 코드 비하인드를 사용하여 동적으로 제어 가능.

**MainWindow.xaml**<br>
현재 프로젝트의 메인화면.<br>
MainWindow.xaml.cs 파일 코드 비하인드를 사용하여 동적으로 제어 가능.<br>

**Assemblyinfo.cs**<br>
해당 프로젝트의 어셈블리 정보를 포함하고 있음.<br>
보통 특별한 상황 아닌 이상 건드릴 필요X<br>

**종속성**<br>
현재 프로젝트에서 참조하는 라이브러리 혹은 다른 프로젝트를 표시함

## 컨트롤 도구 상자

보기 > 도구 상자 > 공용 WPF 컨트롤 & 모든 WPF 컨트롤

## 시작 uri

기본 시작 uri는 MainWindow.xaml<br>
App.xaml 파일 > StartupUri 변수의 값을 변경하면 됨

## 실행 중인 앱 상단의 위젯

디버깅 기능 제공하는 런타임 도우미임<br>
첫 번째 단추 **라이브 시각적 트리**로 페이지의 모든 시각적 요소가 포함된 트리 확인 가능

## 릴리스 버전 빌드

1. 기본 메뉴에서 **빌드 > 솔루션 정리**를 선택하여 이전 빌드 과정에서 만들어진 중간 파일과 출력 파일을 삭제
2. 도구 모음에서 빌드 구성을 **디버그**에서 **릴리스**로 변경
3. **빌드 > 솔루션 빌드**를 선택하여 솔루션을 빌드

<br>

# DXF 포맷

DXF 파일 포맷은 실제 AutoCAD drawing을 위한 데이타구조가 아니며, 외부와의 변환, 전환을 위한 특정 포맷이다. DXF는 표준 ASCII 포맷으로 되어 text editor를 통해서도 쉽게 인식할 수 있다. 그리고 다른 프로그램에 의해 쉽게 읽혀질 수 있도록 설계된 포맷으로 1라인당 하나의 필드로 구성되어 가장 쉽게 유지되는 반면 그만큼 파일크기가 방대해지는 단점을 가진다. 파일의 구성에 있어서 **첫 라인은 그룹코드(Group Code)가 나오고, 두번째 라인은 그 그룹의 값이 나오고, 이렇게 두 라인씩 쌍을 이루어 계속 반복된다.**
DXF 포맷은 공간실체의 X,Y,Z 좌표에 대해 각각 다른 그룹코드를 가진다. 예로, X좌표의 그룹코드에 10을 더하면 Y좌표의 그룹코드가 되고, Y좌표의 그룹코드에 10을 더하면 Z좌표의 그룹코드를 얻는다. 한편 DXF파일에서는 **그룹코드 0**이 자주 나타나는데, 이 그룹코드는 **다음 라인부터 새로운 내용이 시작된다는 것을 알리는 역할**을 한다. 그룹코드 0의 정확한 의미는 다음 라인에 무엇이 오는가에 따라 달라진다.
(출처: https://cafe.daum.net/powernct/HCUt/2?q=D_aSce_-d5P550&)

## DXF의 좌표계

### DXF의 OCS(객체 좌표계; Object Coordinate System)

공간을 도면 데이터베이스(및 DXF 파일)에 저장하려면 각 도면요소에 연관된 점은 도면요소 자체의 객체 좌표계(OCS) 항으로 표시된다. OCS에 대해 3D 공간에서 도면요소의 위치를 설명하는 데 필요한 유일한 추가 정보는 OCS의 Z 축을 설명하는 3D 벡터와 고도 값이다.

원점이 WCS의 원점과 일치합니다.
XY 평면 내의 X 및 Y축 방향은 임의의 방식으로 계산될 수 있지만 일관성이 유지되어야 합니다.

### WCS(표준 좌표 시스템; World Coordinate System)

물체의 위치를 표현할 때 기준으로 삼는 실제 공간의 좌표계이다.

### UCS(사용자 좌표계; User Coordinate System)

UCS를 통해 입력된 2D 점은 OCS의 상응하는 2D 점으로 변환되며 UCS를 기준으로 이동되고 회전한다.

<br>

# 라이브러리

라이브러리 프로젝트로 작업하여 .dll 라이브러리 파일로 만들면 팀원들에게도 배포 가능! <br>
솔루션 탐색기의 참조에 추가하여 사용할 수 있다.

## 참고한 라이브러리

### [netDXF](https://github.com/haplokuon/netDxf/tree/master)

엔티티를 dxf 포맷의 파일로 저장하고 불러오는 기능을 제공한다.<br>

### [WPF UI](https://github.com/lepoco/wpfui)

WPF UI 라이브러리를 제공한다.

<br>

# .NET 생태계

### .Net Core

윈도우, 리눅스, macOS 사용 가능합니다.

- 모든 운영 체제에 대한 응용 프로그램을 구축하기 위한 새로운 오픈 소스 및 크로스 플랫폼 프레임 워크 입니다.
- UWP 및 ASP.NET Core 만 지원합니다.
- ASP.NET Core는 브라우저 기반 웹 응용 프로그램을 작성하는 데 사용합니다.
- 경량화로 인해 매우 가볍다
- Docker 사용 가능

### .NET Core 이점

새로운 프로그램을 구축해야 한다면 .NET Framework보다는 .NET Core이 좋습니다.

- 마이크로 소프트사에서는 .NET Core v3.0을 발표했습니다.
- 미래의 경우 .NET Core가 될 수 있습니다.
- .NET Core 3.0 발표
- WPF에서 Windows Forms을 지원합니다.
- 즉. UWP/WPF 및 Windows Forms 간의 교차 개발도 지원합니다.
- 이것은 최신 UWP 인터페이스를 Windows Forms 및 WPF로 가져올 수 있는 유연성을 제공합니다.

### .Net Framework

윈도우 및 웹 응용 프로그램을 사용 가능합니다.

- Windows Forms, WPF/UWP를 사용하여 Windows 응용 프로그램을 빌드 할 수 있습니다.
- ASP.NET MVC는 웹 응용 프로그램 작성에 사용합니다.
- 다양한 기능과 확장성을 지원

### .NET Framework 이점

아무것도 모르는 상태에서, 빠르게 진행이 필요 할 경우, .NET Framework를 선택하세요.

- .NET Core의 경우는 학습이 조금 더 어렵습니다.
- .NET Framework는 현재 버전인 4.8이 마지막 버전으로 간주됩니다.
- 지속적인 업그레이드와 변경을 하고 싶지 않을 경우 선택하세요.
- 일하는데 안정적인 환경을 줍니다.

<br>

# 참고 문서

- [다중 폼 함수 호출 전역 Mediator 클래스](https://blog.naver.com/goldrushing/221663952698)

- [.NET Framework와 .NET Core](https://digiconfactory.tistory.com/entry/%EB%8B%B7%EB%84%B7WPF-%EA%B0%9C%EC%9A%94)

- [MVVM 패턴 소개](https://kaki104.tistory.com/830)

- [MVVM 구조 간단한 예시](https://frozenpond.tistory.com/53)

- .NET Framework 공식문서

  - [끌어서 놓기](https://learn.microsoft.com/ko-kr/dotnet/desktop/wpf/advanced/drag-and-drop-overview?view=netframeworkdesktop-4.8)

  - [그리기](https://learn.microsoft.com/ko-kr/dotnet/desktop/wpf/graphics-multimedia/shapes-and-basic-drawing-in-wpf-overview?view=netframeworkdesktop-4.8)
