# 소개

서버에 대해 전혀 모르는 상태로 '서버에 대해 공부하자!'라는 생각으로 시작하게된 프로젝트 입니다.

프로젝트 초기에는 서버와 클라이언트간에 통신하는 코드 구현을 목적으로 시작하였지만, 나중에는 실제 프로덕션 환경에서 사용할 수 있는 게임서버 구현을 목표로 하게 됬습니다.

많은 회사들이 클라우드 환경을 선택하고 있기 때문에, 클라우드 환경에서 동작하는 것을 목표로 구현했습니다.

다음의 기사를 참고로 하여 프로젝트를 진행하였습니다.

[기사1](https://qiita.com/naoya-kishimoto/items/0d913a4b65ec0c4088a6), [기사2](https://qiita.com/naoya-kishimoto/items/8a9a5db30717865d867e)

로컬 환경과 클라우드 테스트 환경에서 정상적으로 작동하는 것을 확인 하였습니다.

밑으로 프로젝트의 구성과 프로젝트를 하면서 알게 된 것 등을 소개합니다.

[테스트영상](https://youtu.be/5-oBiYwNYzo)
## 📖 Table of contents

- [사용기술](#사용기술)
- [인프라](#인프라)
- [프로젝트 초기](#프로젝트-초기)
    - [MagicOnion](#MagicOnion)
    - [UniTask](#UniTask)
    - [Addressable](#Addressable)
- [단일서버->다중서버](#단일서버에서-다중서버로-변경시-문제점)
- [매칭 시나리오](#매칭-시나리오)
- [Kubernetes/Agones](#KubernetesAgones)
- [HTTPS](#HTTPS)
- [배포 시나리오](#배포-시나리오)
- [DB](#DB)
- [BakingSheet & Addressable](#BakingSheet--Addressable)
- [MessagePipe](#MessagePipe)
- [VContainer](#VContainer)
- [참고한링크](#참고한-링크)

## 사용기술

### 클라이언트
- Unity
  
### 서버
- ASP.Net Core
- EF Core
- Redis
- SQL Server
- Docker
### 공통
- Dotnet,C#
- MagicOnion
- MessagePipe
- BakingSheet

### 인프라

- AWS
  - ElasticKubernetesService (EKS)
  - ElasticContainerRegistry (ECR)
  - SecretsManager
  - CodeBuild

## 프로젝트 초기

사용언어로 C#을 선호하기 때문에, C#을 활용 할 수 있는 기술을 선택했습니다.

클라이언트로 Unity,서버로 ASP.NET Core, 통신 프레임워크로 MagicOnion을 사용했습니다.

클라이언트 비동기 코드에 UniTask를 활용 했고, Addressable 등 사용해보고 싶었던 기술들을 추가 하기로 했습니다.

## MagicOnion
(https://github.com/Cysharp/MagicOnion)

MagicOnion은 gRPC와 MessagePack기반의 통신 프레임워크로, C# Interface 코드를 공유하는 것으로 간단하게 서버,클라이언트간 통신 프로그램을 작성할 수 있습니다.

통신 방식으로 Service와 Hub 두가지 방식이 지원되는데, 양방향 통신 방식의 Hub가 실시간 게임서버 구현에 적합합니다.

Group단위로 브로드캐스트하는 등 사용자가 직관적으로 쉽게 사용할 수 있고 Github의 Readme를 조금 읽은 것 만으로 서버,클라이언트간 통신 프로그램을 쉽게 작성 할 수 있었습니다.

서버와 클라이언트 모두 C#코드를 공유해야하는 구조상 자연스럽게 Client,Server,Shared 구조로 나뉘게되며 Server에서 Shared프로젝트를 참조하고 Shared에서 Unity내의 폴더를 링크하는식으로 디렉토리 구조가 잡히게 됩니다.

csproj파일을 건드리는것으로 간단히 폴더를 공유할 수 있고, [SlnMerge](https://github.com/Cysharp/SlnMerge)를 이용하여 하나의 Solution으로 Unity project와 Server Project를 연결 할 수 있습니다

## UniTask
(https://github.com/Cysharp/UniTask)

Unity에서 async/await 패턴을 사용할 때 Task보다 가볍습니다.

Task와의 성능차이에 관심이 없어도, PlayerLoop의 타이밍을 조절하거나 Addressable,Dotween같은 다른 라이브러리와의 통합으로 await 할 수 있게 되는 등 굉장히 편리합니다.

## Addressable

Unity용 에셋관리 시스템입니다.

주 목적인 메모리 관리기능보다 코드 어디서든 에셋을 찾아서 쓸 수 있다는 장점이 더 부각됬습니다.

다만, Remote Build/Load시 번들 업로드가 번거로웠기에, Local Build로 테스트하고 다른부분을 먼저 신경썻습니다.

## 단일서버에서 다중서버로 변경시 문제점

단일 로컬서버로 테스트하는데 성공했으나 다중서버로 변경시 몇가지 문제점이 생겼습니다.

우선 클라이언트가 통신해야할 서버를 어떻게 찾아야 하는지 몰랐고, 각각 다른서버에서 통신중인 두 유저사이의 데이터를 어떻게 공유할 것인지가 문제입니다.

단일 진입점으로 여러 서버에 연결을 분산해주는 Load Balancer의 존재와 Redis같은 외부 스토리지를 통해 데이터를 공유할 수 있다는것을 활용해 코드를 구현했지만,외부 스토리지를 활용하는 코드가 직관적이지 않았고 데이터를 활용하는데 제약이 있었습니다. 또한, 외부로 패킷을 보내야하기에 발생하는 시간 과 비동기 처리가 만족스럽지 않았습니다.

추후 서비스 디스커버리 방식으로 게임서버와 직접 통신하는 방식을 채택하면서 코드를 상당부분 재작성하게 됐습니다.

## 매칭 시나리오

매칭 서버를 이용한 로그인 시나리오는 다음과 같습니다.

1. Client가 계정생성 요청 시,로그인 서버는 DB에 계정 생성
2. Client가 로그인 요청 시,로그인 서버는 LoginToken생성 후 Redis에 등록하고 Client에 반납합니다. 추가로 Pub/Sub기능을 이용해 다른서버의 연결을 끊습니다.
3. Client가 반환받은 LoginToken을 사용해 게임 서버에 진입 요청을 합니다. 게임 서버는 Redis를 확인하여 로그인된 유저인지 확인합니다.

매칭 서버를 이용한 멀티플레이 매칭 시나리오는 다음과 같습니다.

1. Client가 매칭서버에 RoomList 를 요청합니다. 요청시 조건Filter를 추가할 수 있습니다.
2. 매칭서버는 RoomList 를 각 게임서버마다 요청하여 필터링한 후 반환합니다.
3. 요청받은 게임 서버는 RoomList를 반환 합니다. RoomList에는 roomId,room내 플레이어 정보,Room에 접속하기위한 게임 서버의 Address가 있습니다.
4. 유저가 멀티플레이RoomList에서 접속할 room을 선택하면, Client가 게임서버에 접속 요청을 보냅니다.

## Kubernetes/Agones

(https://qiita.com/naoya-kishimoto/items/8a9a5db30717865d867e) 참조

![image](https://github.com/heartviolence/GameServerTest/assets/127963333/c4674f28-4122-4386-9a78-4f556934f609)

![image](https://github.com/heartviolence/GameServerTest/assets/127963333/8a274a6a-0578-46fd-8969-ce974a67d6e4)

## HTTPS

GameServer배포 시 연결 Address가 다르게 나올 수 있습니다.

각각의 Address를 위한 인증서를 제공할 수 없기 때문에, *.domain.com로 인증서를 발급받고, *.domain.com의 요청을 GameServer의 Address로 라우팅하는 방식으로 하나의 인증서를 모든 서버끼리 공유합니다.

라우팅을 구성하는 방식으로는 서버에서 AWS SDK를 사용하여 Route53 호스트영역에 레코드를 생성하기로 했습니다.

AWS Route53를 통해 도메인 구입 시 무료 인증서를 제공 받을 수 있습니다만, 개인키를 제공 받을 수 없기에 프로젝트에서 사용하기 어렵습니다.금전적인 문제 때문에 토이프로젝트임을 감안해,  자체인증 인증서를 사용하기로 했습니다.

테스트할 환경에서 인증CA를 등록하면, 보안 경고없이 HTTPS를 사용해 서버와 통신할 수 있습니다.

서버에게 인증서를 공급할 방법으로 디렉토리에 합쳐서 빌드하는 방법이 있지만, Github에 민감한 정보를 올리고 싶지 않을 경우, Secret Manager등 외부 비밀관리자를 통해 받아올 수 있습니다.

이 프로젝트에서는 appsettings.json파일을 이용하여 빌드 환경별로 사용할 인증서를 다르게 합니다.

## 배포 시나리오

AWS간 리소스연동이 간단하기 때문에, 모든 리소스를 AWS에 구축했습니다.

AWS EKS를 사용하면 클러스터 구성에 많은 관리가 필요한 부분을 대신해주어 시간과 노력을 많이 절약할 수 있습니다.

최종적인 구성은 다음과 같습니다.

![도식](https://github.com/heartviolence/GameServerTest/assets/127963333/743855b9-bd5c-4ff8-9950-d0e46aa192d2)


빌드 시나리오는 다음과 같습니다.

![도식2](https://github.com/heartviolence/GameServerTest/assets/127963333/16988d09-34f2-4c5c-9871-9744c47e967d)


CodeBuild 프로젝트를 두개 만들어, Test Build와 Product Build를 구분합니다

![image](https://github.com/heartviolence/GameServerTest/assets/127963333/b6855533-7dfb-43c2-8b4a-cf568f52ca79)


각각의 BuildSpec은 매치서버와 게임서버의 Dockerfile을 찾아 build하고 다른 Tag로 ECR에 업로드합니다.

(Dockerfile중 일부)

![image](https://github.com/heartviolence/GameServerTest/assets/127963333/1bc0434b-d384-4d0e-96cd-cbe5debce508)

(csproj파일의 constant를 선언하는 부분)

![image](https://github.com/heartviolence/GameServerTest/assets/127963333/10386c97-b7f5-43dd-b713-782b81fa0613)

build시 각기 다른 인수를 넘겨줄 수 있기에 빌드 환경마다 다른 상수를 선언할 수 있고, 전처리문을 이용하여 환경마다 다른 코드를 사용할 수 있습니다.

게임 서버의 버전 업그레이드 시 롤링 업데이트로 무중단 배포가 가능하지만 서로 다른 버전의 서버가 공존해서는 안되는 상황이나 DB등 다른 시스템과의 호환 문제로 모든 시스템을 중단하고 배포한다고 가정합니다.

## DB

사용자의 데이터를 보존과 서버간 데이터공유를 위해 DB를 사용합니다. 

RDB는 스키마를 통해 무결성을 지원하지만, 반대로 스키마에 존재하지 않는 데이터는 저장할 수 없기에 다양한 활용에 제약이 생깁니다. 따라서 Redis같은 NoSql을 병행 하기로 하였습니다. 유저 Account에 관한 데이터는 SQLServer에 저장하고, 그 외 잃어버려도 치명적이지 않거나 서버간 공유를위한 휘발성 데이터는 Redis에 저장하기로 하였습니다.

Redis는 데이터 저장외에도 Pub/Sub기능을 지원하므로, 이를 통해 서버를 연결할 수 있습니다.

서버가 늘어날수록 DB에 대한 부하도 늘어나므로, 클러스터링하거나 데이터를 수평 분할하는 것으로 부하를 분산시켜주어야 합니다.

해당 프로젝트에선 Acccount별로 데이터를 분할할 수 있지만, DB의 고가용성을 유지하기 위해선 많은 노력이 필요하고 인프라적인 부분에 많은 노력을 쏟고 싶지 않았기에 AWS RDS를 사용하는 등 적당히 타협했습니다.

C#코드에서 Redis를 이용하기 위해 Cloudstructures를 사용합니다. stackexchange.redis를 그대로 사용하는 것 보다 훨씬 편리합니다.

빠른개발을 위해 ORM으로 EFCore를 사용하였는데, 코드 퍼스트로 개발시 자동으로 스키마를 구성해준 덕분에 DB작업에 시간과 노력을 절약할 수 있었습니다.

여러 서버가 동시에 DB를 작업하게되면 동시성 문제가 발생하는데, 프로젝트 구조상 작업이 단일 게임서버에서 발생하기에 서버 내에서 Lock을 사용하는것으로 해결 가능 했습니다.

그 외 작업이 빈번하지 않으면서 동시성 문제가 발생하는경우 SQLServer에서 지원하는 낙관적동시성모델을 사용하여 해결 가능 합니다.

## BakingSheet & Addressable

인게임 내 데이터를 관리하기 위하여 Excel을 사용하기로 했습니다.

게임 개발시 협업을 위해 코드와 데이터를 분리하는 것이 좋고, 데이터를 관리하기 위한 전용 툴을 제공하는 것이 최고지만, 결국 Excel을 사용하기로 했습니다.

Excel로 데이터를 입력하고 .xlsx파일을 Unity로 가져오게되면, 자동으로 json파일로 변환하여 서버 프로젝트에 로드합니다.

Github에 push하고 build하여 배포하게되면 클라이언트는 서버에서 데이터파일을 다운로드 받을 수 있습니다.

서버와 클라이언트 모두 json 파일을 BakingSheet로 읽어들여 C#코드 내에서 사용합니다.

Addressable로 빌드한 에셋번들과 카탈로그도 서버디렉토리로 빌드하여 Github에 push하게되면, 서버에 업로드하는 추가 작업없이 기존 배포 프로세스로 배포할 수 있습니다. Client코드와 Server코드를 같은 repository에 push하기에 버전관리 또한 간편합니다.

## MessagePipe

(https://github.com/Cysharp/MessagePipe)

MagicOnion의 Hub간의 통신을 위해 MessagePipe를 사용했습니다.

MessagePipe는 EventAggregator패턴을 지원하며, 이를 이용해 각기다른 Hub끼리 서로를 모르더라도 이벤트를 구독/발행 할 수 있습니다.

Server코드에서 먼저 사용해 보고, 클래스간 연결이 느슨해지는 부분을 고평가해 클라이언트 코드에도 적용시켰습니다.

Unity에서 MessagePipe를 사용하기 위해서는 VContainer 또는 Zenject가 필요합니다.

## VContainer

(https://github.com/hadashiA/VContainer)

Unity용 DI라이브러리 입니다. 

클라이언트 코드에서 Messagepipe를 사용하기 위해 도입했으나, 기존 코드도 DI를 사용해 재작성 했습니다.

VContainer를 사용하면, 코드 제어 흐름을 관리하기 편하고 필요한 객체 전달이 굉장히 쉬워집니다.

코드를 재작성하는 과정에서 Model과 View를 분리했습니다. UniTask의 PlayerLoop제어와 병행하면 MonoBehavior의 제어흐름에 의존하지 않고도 세밀한 타이밍 조절이 가능합니다.

View클래스는 View에 관한 로직만을 구현하고, Presenter클래스가 View와 Model을 직접 조작합니다. DI를 사용해 Presenter클래스에 Model과 View를 손쉽게 주입 할 수 있습니다.


## 참고한 링크

Agones (https://agones.dev/site/docs/)

external-dns (https://repost.aws/ko/knowledge-center/eks-set-up-externaldns)

EKSTutorial(https://docs.aws.amazon.com/eks/latest/userguide/getting-started.html)

MagicOnion (https://github.com/Cysharp/MagicOnion)

MessagePipe (https://github.com/Cysharp/MessagePipe)

slnMerge (https://github.com/Cysharp/SlnMerge)

BakingSheet (https://github.com/cathei/BakingSheet)

DBup (https://dbup.readthedocs.io/en/latest/)

MagicOnion에서 시작하는 실시간 통신 전편 (https://qiita.com/naoya-kishimoto/items/0d913a4b65ec0c4088a6)

MagicOnion에서 시작하는 실시간 통신 후편 (https://qiita.com/naoya-kishimoto/items/8a9a5db30717865d867e)

Vcontianer (https://vcontainer.hadashikick.jp/)

Cloudstructures (https://github.com/xin9le/CloudStructures)
































