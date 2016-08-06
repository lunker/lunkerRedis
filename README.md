# Chatting Project#1 Backend Server Application 
Team [혜진-태우-동규]  
4:33 intern Chatting Project-BackEnd Redis Server

## 개발 환경 
- Visual Studio 2015
- MySQL 5.7.13
- Redis 3.2.3
- log4net 1.2.15

## 실행 환경
- NET 4.5.2 이상  

## Redis 설계 문서 
- 'master'branch /design/  

## Dependencies
- log4net : logging
- StackExchange.Redis : redis client(https://github.com/StackExchange/StackExchange.Redis)   
- mysql connector (https://dev.mysql.com/downloads/connector/net/6.9.html)  

## 기능
- Frontend Server, Monitoring Client의 요청을 처리하기 위하여 각각의 Listener를 구현.
- Backend Server에 접속한 Frontend Server들의 정보들을 동적으로 관리합니다.
- Frontend Server가 이탈시, 해당 Frontend Server와 관련된 cache는 삭제됩니다.(채팅내역제외)
- User의 접속 여부, 채팅 횟수 등을 관리합니다.
- 채팅룸, 채팅 횟수, 채팅방의 입장인원 등의 정보 cache  
- Monitoring 기능을 제공합니다. 1) 전체 채팅방 수 조회 2) FE별 사용자수 조회 3) 채팅 랭킹 조회   
- Frontend Server와는 HealthCheck를 수행하여 disconnect를 체크하고 있습니다.
- Monitoring Client는 5초에 한번씩 조회 요청을 보내기에, 별도의 Heatlcheck를 구현하지 않았습니다.

## 실행방법
0) Release Binary 다운로드  
1) VM 환경 구성  
2) 환경설정  
3) 실행  


### 실행방법 - Release Binary 다운로드
1) 'release' branch에서 'release.7z'을 다운로드 

### 실행방법 - VM 환경 구성 

1) MySQL - 5.7.13 GA
- 가상 호스트 전용 어답터를 사용.  
- Release/sql/User.sql의 Query 실행하여 관련 Table Setup   

  
2) Redis
- 가상 호스트 전용 어답터를 사용.
- 별도의 설정없이 기본 설치 후 사용.

### 실행방법 - 환경설정 

1) MySQL config 설정
- path : config/MySQLConfig.xml
- database, connectiontimeout을 제외한 나머지 정보들을 알맞게 수정.


```
<MySQLConfig>
  <server>192.168.56.190</server>
  <uid>lunker</uid>
  <pwd>dongqlee</pwd>
  <database>chatting</database>
  <ConnectionTimeout>600</ConnectionTimeout>
</MySQLConfig>
```


2) Redis config 설정
- path : release/config/RedisConfig.xml    
- ip, port 수정
- ip : 가상 호스트 전용 ip (192.168.56.xxx) 
- port : port

```
<RedisConfig>
  <ip>192.168.56.102</ip>
  <port>6379</port>
</RedisConfig>
```


3) Application Config 설정
- path : release/config/AppConfig.xml  
- client, frontend Server의 listen port 지정


```
<AppConfig>
  <clientListenPort>20852</clientListenPort>
  <frontendListenPort>25389</frontendListenPort>
</AppConfig>
```

4) log4net Config 설정 
- path : release/config/LogConfig.xml
- 아래 항목 중 'file'을 수정하여 원하는 로그 파일 저장 경로 설정. 

```
<log4net>
  <appender name="exlog" type="log4net.Appender.RollingFileAppender">
    <file value="c:\log\" />
    <datePattern value="yyyy-MM-dd'_exlog.log'"/>
    <preserveLogFileNameExtension value="true" />
    <staticLogFileName value="false" />
    <appendToFile value="true" />
    <rollingStyle value="Composite" />
    <countDirection value="1"/>
    <maxSizeRollBackups value="100" />
     <maximumFileSize value="100MB" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value = "[%-5level][%-5thread][%-25method] : %date{HH:mm:ss,fff}   %message%newline"/>
    </layout>
  </appender>
  <logger name = "Logger">
    <level value="Info" />
    <level value="Debug" />
    <appender-ref ref="exlog"/>
  </logger>
</log4net>
```

### 실행 
- MySQL VM 실행 및 mysqld 실행 
- Redis VM 실행 및 redis.service 실행
- release/LunkerRedis.exe 실행 

## 사용법 및 주의사항
- 실행파일을 실행 후, Console에서는 진행 사항이 출력되지 않습니다.
- 해당 진행 사항내역은 로그파일을 통해서 확인할 수 있습니다.
- 진행 내역을 확인하기 위하여, 어플리케이션이 종료되면 Redis의 cache내역을 모두 삭제합니다. 
- 단, FE의 접속이 끊길경우 해당 FE관련 정보들만 삭제되며 Backend Server는 정상 진행됩니다.
- 종료는 Console 창의 안내에 따라 종료해주십시오.


