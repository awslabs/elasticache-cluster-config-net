# Amazon ElastiCache Cluster Configuration for .NET Standard 2.0

Amazon ElastiCache Cluster Configuration is an enhanced .NET library that supports connecting to an Amazon ElastiCache cluster for Auto Discovery. This client library is an extension built upon Enyim and is released under the [Apache 2.0 License](http://aws.amazon.com/apache2.0/).

## Usage

Because of the changing for EnyimMemcachedCore interfaces, we have to pass the ILoggerFactory instance additionally.

You should use DependencyInjection with WebHost or GenericHost.
    
```cs
ILoggerFactory loggerFactory = ...;
var client = new MemcachedClient(loggerFactory, new ElastiCacheClusterConfig(loggerFactory, "hostname", 11211));
```

### Configuration

Unlike the original version of ElastiCacheClusterConfig, this branch has no implicit loading configuration feature.

If you want to set configuration, you should set up  `ClusterConfigSettings` manually.

### Misc

I recommend you to check out the list below:

- [Original Repository](https://github.com/awslabs/elasticache-cluster-config-net)
- [EnyimMemcachedCore](https://github.com/cnblogs/EnyimMemcachedCore) Using this library inside.

## Requirements

You'll need .NET Core 2.2 or later to use the precompiled binaries. To build the client, you'll need .NET Core SDK 2.2.100 or higher.
