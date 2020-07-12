# publisher-api

An API for testing Rabbitmq

## Installation

Step1: Clone repository

```
git clone https://github.com/vandungbkfet12/publisher-api.git
```

Step2: Go to PushlisherAPI\bin\Release\netcoreapp3.1 and Edit configurations in apppsetting.json. Assume that you already have a rabbitmq server which installed on WINDOW

```
  "RabbitMq": {
    "Hostname": "localhost",
    "Port": 5672,
    "QueueName": "test",
    "UserName": "guest",
    "Password": "guest"

  }
  ```
  Change configurations as you want.
  
  Step3: Run PublisherAPI.exe and go to http://localhost:5000/swagger, you can see like this: https://prnt.sc/tg7jg6
  
  Step4: In /rabbitmq/test-multi-concurrrent path. Enter parameters and click Execute.
  
  Step5: If you have no any error, respone is "Completed!". You can check in Rabbimq Management, a queue named "test" creatd with messages. You can see like this: https://prnt.sc/tg7jw7



