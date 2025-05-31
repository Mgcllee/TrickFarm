# Trick Farm

* 서버에서 클라이언트 정보를 관리하기 위해 클라이언트 1개 당 Orleans Grain 1개씩 확보.  
* 각 클라이언트는 채팅 방에 입장한 뒤에 채팅을 시작할 수 있으며, 채팅 방은 클라이언트처럼 **Grain** 으로 관리  
* 클라이언트와 통신하기 위해 서버는 EPoll을 사용해서 소켓 연결  
* EPoll Server가 새로운 클라이언트와 연결되면 Orleans Server에게 소켓 네트워크로 알림
* Redis로 현재 접속한 클라이언트의 Guid 값과 채팅에 표시될 이름을 함께 저장.
* 각 Grain에서 Redis에 접근하기 위해 Redis를 관리하는 class를 Singleton 객체로 관리

> 최신 릴리즈 버전 위치: [TrickFarm GitHub](https://github.com/Mgcllee/TrickFarm/tree/570f15198015d8f055f49818490b9c0d26398ffa/TrickFarmServer)

> Trick Farm 리뷰 글: [mgcllee 블로그](https://mgcllee.github.io/categories/trick-farm/)

<br/>

## Orleans with Dashboard, Azure VM(Ubuntu), Blazor 클라이언트트 실행 화면

> **이미지를 클릭하시면 큰 이미지에서 확인하실 수 있습니다.**

![실행화면](/Document/Image/TrickFarmServer_runtime_0414.png)  

<br/>

---

#### 접속 방법

1. 아래의 주소를 웹 브라우저로 접속 (대시보드는 http를 사용하므로 접속에 주의 필요)
2. 클라이언트의 메뉴 중 'Home', 'Counter', 'Weather'는 접속이 가능  
   'Chat'은 Orleans 서버가 동작일 때 접속 가능  
3. **(필수)**'Chat' 페이지에 접속하면 채팅 입력 칸에 아래와 같은 순서로 입력('Enter'키 혹은 전송 버튼으로 송신)  
   a. 사용할 '이름' 입력  
   b. 'join '과 함께 입장할 방 이름 입력 (ex. join 초보만 대환영)  
   c. 자유롭게 채팅 입력 가능  
   d. 채팅방을 떠날 경우, 'leave'만 입력하여 채팅방 퇴장 가능  

<br/>

* 현재 개발이 진행 중인 프로젝트로 **비정상적 동작이 있을 수 있습니다.**
* Azure VM 크기가 작아 **반응속도가 느릴 수 있습니다.**

Blazor Client URL: https://trickfarmweb.koreacentral.cloudapp.azure.com  
  
Orleans Dashboard URL: http://trickfarm-orleans.koreacentral.cloudapp.azure.com:8080/dashboard  
(Orleans Dashboard는 편의를 위해 HTTPS가 아닌 HTTP를 사용, 접속시 주의)  

---

<br/>

## Trick Farm 구조도

<p align="center"><img src="/Document/Image/TrickFarm_구현도_03.png" width="600" height="600"></p>

<br/>

<p align="center"><img src="/Document/Image/NotifySlack.png" width="300" height="150"></p>

<p align="center">[Git Commit, GitHub Action 결과를 Slack으로 받은 결과]</p>

<br/>

|VM 이름|역할|크기|OS|
|---|---|---|---|
|TrickFarmServer|Client 객체 관리 및 Redis 정보 관리| 2vCPU, 1GiB 메모리|Ubuntu server 22.04 LTS|
|TrickFarmWebApp|Web 요청 처리 및 TrickFarmServer 연결| 2vCPU, 1GiB 메모리|Ubuntu server 22.04 LTS|

<br/>

## Trick Farm 개발 과정

<p align="center"><img src="/Document/Image/TrickFarm_개발순서도_03.png" width="500" height="500"></p>
