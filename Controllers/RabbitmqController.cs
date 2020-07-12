using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PushlisherAPI.Models;
using RabbitMQ.Client;

namespace PushlisherAPI.Controllers
{
    [ApiController]
    [Route("rabbitmq")]
    public class RabbitmqController : ControllerBase
    {

        private readonly ILogger<RabbitmqController> _logger;
        private readonly IOptions<RabbitMqConfiguration> _config;
        public RabbitmqController(ILogger<RabbitmqController> logger, IOptions<RabbitMqConfiguration> config)
        {
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// Health check
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Still alive!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n">Number of concurrent</param>
        /// <param name="delay">Delay time</param>
        /// <param name="total">total of messages</param>
        /// <returns></returns>
        [HttpPost("test-multi-concurrrent")]
        public IActionResult TestMultiConcurrent([FromQuery] int n, int delay, int total)
        {
            try
            {

                try
                {
                    //Check rabbitmq connection
                    var cf = _config.Value;
                    var factory = new ConnectionFactory() { HostName = cf.Hostname, Port = cf.Port, UserName = cf.UserName, Password = cf.Password };
                    var connection = factory.CreateConnection();
                }
                catch (Exception)
                {
                    return Ok("Init Rabbit Connection failed! Please re-check configurations");
                }                

                //Start threads
                List<Task> taskList = new List<Task>();
                for (int i = 0; i < n; i++)
                {
                    int param = i;
                    int pDelay = delay;
                    int pTotal = total;
                    var task = Task.Run(() => CreateThread(param, pDelay, pTotal));
                    taskList.Add(task);
                }
                Task.WaitAll(taskList.ToArray());
                return Ok("Completed!");

            }
            catch (Exception ex)
            {

                return Ok(ex.Message);
            }

        }
        /// <summary>
        /// Start connect Rabbitmq and send data
        /// </summary>
        /// <param name="i"></param>
        [NonAction]
        public async void CreateThread(int i, int delay, int total)
        {
            try
            {

                var cf = _config.Value;                

                //Init rabbit connection
                var factory = new ConnectionFactory() { HostName = cf.Hostname, Port = cf.Port, UserName = cf.UserName, Password = cf.Password};

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {                    
                    channel.QueueDeclare(queue: cf.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                    //Start sending data
                    for(int j = 0; j < total; j++)
                    {
                        var data = new { clientId = i, timestamps = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") };

                        var json = JsonConvert.SerializeObject(data);
                        var body = Encoding.UTF8.GetBytes(json);

                        channel.BasicPublish(exchange: "", routingKey: cf.QueueName, basicProperties: null, body: body);

                        await Task.Delay(delay * 1000);
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }
}
