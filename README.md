# Trick Farm

![실행화면](/Document/Image/TrickFarmServer_runtime_0414.png)  
[Orleans with Dashboard, Azure VM(Ubuntu), Blazor 클라이언트트 실행 화면]

[개발 계획표](https://github.com/users/Mgcllee/projects/4)

* 서버에서 클라이언트를 관리하기 위해 **Grain** 클라이언트 당 1개씩 할당.  
* 각 클라이언트는 채팅 방에 입장한 뒤에 채팅을 시작할 수 있으며, 채팅 방은 클라이언트처럼 **Grain** 으로 관리  
* 클라이언트와 통신하기 위한 TcpClient 멤버는 Singleton 객체로 관리  
* Redis로 현재 접속한 클라이언트의 Guid 값과 채팅에 표시될 이름을 함께 저장.
* 각 Grain에서 Redis에 접근하기 위해 Redis를 관리하는 class를 Singleton 객체로 관리  

<br/>

# Trick Farm 구조도

![구조도](/Document/Image/TrickFarm_구현도_03.png)

<center>[TrickFarm 구현도]</center>

|VM 이름|역할|크기|OS|
|---|---|---|---|
|TrickFarmServer|Client 객체 관리 및 Redis 정보 관리| 2vCPU, 1GiB 메모리|Ubuntu server 22.04 LTS|
|TrickFarmWebApp|Web 요청 처리 및 TrickFarmServer 연결| 2vCPU, 1GiB 메모리|Ubuntu server 22.04 LTS|

* Azure virtual network, Azure Container Registry, Azure Public IP 활용.  

<br/>

# (예정) Trick Farm 개발 과정

![유저접속과정](/Document/Image/TrickFarm_개발순서도_03.png)

* (추가 예정) GitHub Action을 이용한 자동화 이용.
