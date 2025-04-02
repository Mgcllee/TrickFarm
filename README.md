<h1>Trick Farm</h1>

![실행화면](/Document/Image/TrickFarmServer_runtime_0402.png)  
<center>[서버, 클라이언트 콘솔 실행 화면]</center>

* 서버에서 클라이언트를 관리하기 위해 **Grain** 클라이언트 당 1개씩 할당.  
* 각 클라이언트는 채팅 방에 입장한 뒤에 채팅을 시작할 수 있으며, 채팅 방은 클라이언트처럼 **Grain** 으로 관리  
* 클라이언트와 통신하기 위한 TcpClient 멤버는 Singleton 객체로 관리  
* Redis로 현재 접속한 클라이언트의 Guid 값과 채팅에 표시될 이름을 함께 저장.
* 각 Grain에서 Redis에 접근하기 위해 Redis를 관리하는 class를 Singleton 객체로 관리  

<br>

<p align="center">
  <h1>기술 스택</h1>
  <img src="https://github.com/Mgcllee/TrickFarm/blob/main/Document/Image/TrickFarm_%EA%B5%AC%EC%83%81%EB%8F%84.png" width="600px"> 
</p>
