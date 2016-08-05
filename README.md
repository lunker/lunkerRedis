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
- User의 state 저장 
- 채팅룸, 채팅 내역 등의 정보 cache
- Monitoring 기능 제공 
- 

## 실행방법
1) VM 환경 구성  
2) 환경설정  
3) 실행  

### 실행방법 - VM 환경 구성
1) MySQL - 5.7.13 GA
- 가상 호스트 전용 어답터를 사용.    
- 
- Release/sql/User.sql의 Query 실행

2) Redis
- 

### 실행방법 - 환경설정 

1) MySQL config 설정
-  

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
- 
```
<AppConfig>
  <clientListenPort>20852</clientListenPort>
  <frontendListenPort>25389</frontendListenPort>
</AppConfig>
```


### 실행 
- release/
