# Chatting Project#1 Backend Server Application 

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
- User의 접속 여부, 채팅 횟수 등을 관리합니다.
- 채팅룸, 채팅 내역 등의 정보 cache  
- Monitoring 기능을 제공합니다. 1) 전체 채팅방 수 조회 2) FE별 사용자수 조회 3) 채팅 랭킹 조회   


## 실행방법
1) VM 환경 구성  
2) 환경설정  
3) 실행  

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


### 실행 
- MySQL VM 실행 및 mysqld 실행 
- Redis VM 실행 및 redis.service 실행
- release/LunkerRedis.exe 실행 
