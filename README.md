# RPool.NET
这是一个为.NET提供多线程R语言运算的轻量级框架。
使用命令行自行R语言，并提供线程池管理，重复利用R进程
## 使用说明
### 第一步
请您自行安装R环境，本框架对R版本没有要求。安装完毕后，最好在系统环境变量的Path中增加R安装目录，增加后，后续在配置文件中就无需指定R程序的路径。
### 第二步
导入RPool.NET到项目中，配置项目的web.config或app.config文件，结构如下：
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration> 
  <configSections>
    <section name="rPoolSetting" type="RPool.NET.Configuration.PoolSetting,RPool.NET" />
  </configSections>
  <rPoolSetting  
    limitLife="60"
    limitTimes="100" 
    minPoolSize="3"  
    maxPoolSize="10" 
    workingDirectory="C:\R"
    rDirectory="C:\Program Files\R\R-3.5.3\bin\i386\R.exe"
    ></rPoolSetting>
</configuration>
```
其中，rPoolSetting中各个配置如下：
1. limitLife: R进程最长寿命，默认值60
2. limitTimes：单个R进程最大执行的任务次数限制，默认值100
3. minPoolSize：最少开启多少个R进程，默认3
4. maxPoolSize：最多开启多少个R进程，默认10
5. workingDirectory：工作目录，必填，如果需要取数据，输出数据，设定工作目录后，默认在工作目录读取和输出文件
6. rDirectory：R.exe文件所在目录，如果配置了系统变量，则这里不用配置，否则需要指定R.exe的完整路径

### 第三步
调用Pool初始化R进程池
```c#
 Pool.InInitialize<PoolItemWithLock>();
``` 

### 第四步
调用，执行计算，可以取回R执行输出的结果，演示两种调用方式：
```c#
var result=Pool.ExecuteR("print('你好')").Result;
//[1] "你好"
```
```c#
var result=Pool.ExecuteR("print(args)","你好").Result;
//[1] "你好"
```



## 高级应用
如果需要导入R的各种包，直接在执行要执行的语句中导入即可。
如果不想每次都在要执行的语句中导入，可以创建自己的PoolItemWithLock，下面以每次创建R进程自动导入car包为例：    
创建一个新的类，继承PoolItemWithLock，重写OnInitialize
```c#
    public class MyProcess : RPool.NET.PoolItemWithLock
    {
        public override void OnInitialize()
        {
            ExecuteCommand("library(car)", true, null);
        }
    }
```
初始化时用MyProcess实例化
```c#
 Pool.InInitialize<MyProcess>();
``` 
