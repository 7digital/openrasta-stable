using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Web;
using NUnit.Framework;
using OpenRasta.Configuration;
using OpenRasta.Diagnostics;
using OpenRasta.Hosting;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Pipeline;
using OpenRasta.Pipeline.Diagnostics;
using OpenRasta.Web;
using StructureMap;

namespace OpenRasta.DI.StructureMap.Tests.Unit {
	
		[TestFixture]
		public class ConcurrencyTests {
			private static StructureMapDependencyResolver _resolver;

			[Test]
			public void Should_not_throw_exception_when_multiple_threads_manipulate_the_Dependency_resolver() {
				int errorCount = 0;

				for(int i=0; i<1000; i++ ) {
					
					new Thread(() =>{
						var container = new Container();
						container.Configure(c => c.Scan(s => {
							s.AssemblyContainingType(GetType());
							s.LookForRegistries();
							s.WithDefaultConventions();
						}));

						_resolver = new StructureMapDependencyResolver(container);
						_resolver.AddDependency(typeof(IDependencyResolver),
												typeof(StructureMapDependencyResolver),
												DependencyLifetime.Singleton);
						_resolver.AddDependency(typeof(IPipeline), typeof(PipelineRunner),
												DependencyLifetime.Singleton);
						_resolver.AddDependency(typeof(ILogger), typeof(NullLogger),
												DependencyLifetime.Singleton);

						try {
							_resolver.Resolve<IPipeline>();
						} catch (Exception) {
							errorCount++;
							throw;
						}

					}).Start();
				}

				Assert.That(errorCount, Is.EqualTo(0), "Some of the requests threw errors meaning that the dependencyresolver is not threadsafe ");
			}

			[Test, Ignore]
			public void Should_not_throw_exception_if_multiple_threads_inject_conext_into_singleton_dependency_resolver() {
				var container = new Container();
				container.Configure(c => c.Scan(s => {
					s.AssemblyContainingType(GetType());
					s.LookForRegistries();
					s.WithDefaultConventions();
				}));

				_resolver = new StructureMapDependencyResolver(container);

				_resolver.AddDependency(typeof(IDependencyResolver),
										typeof(StructureMapDependencyResolver),
										DependencyLifetime.Singleton);
				_resolver.AddDependency(typeof(IPipeline), typeof(PipelineRunner),
										DependencyLifetime.Singleton);
				_resolver.AddDependency(typeof(ILogger), typeof(NullLogger),
										DependencyLifetime.Singleton);
				
				for (int i = 0; i < 200; i++) {
					int errorCount = 0;
					new Thread(() => {

						try {
							var context = new InMemoryCommunicationContext();
							_resolver.AddDependencyInstance<ICommunicationContext>(context,DependencyLifetime.PerRequest);
							_resolver.AddDependencyInstance<IRequest>(context.Request,DependencyLifetime.PerRequest);
							_resolver.AddDependencyInstance<IResponse>(context.Response,DependencyLifetime.PerRequest);
							_resolver.AddDependencyInstance<IHost>(new InMemoryHost(new ConfigurationSource()), DependencyLifetime.PerRequest);

							_resolver.Resolve<ICommunicationContext>();
							
						} catch (Exception) {
							errorCount = errorCount +1 ;
							throw;
						}

					}).Start();
					Assert.That(errorCount, Is.EqualTo(0), "Some of the requests threw errors meaning that the dependencyresolver is not threadsafe ");
				}
			}
		}

	public class ConfigurationSource : IConfigurationSource
	{
		public void Configure() {
			// do nuttin
		}
	}

	public class DummyHandler : IHttpHandler
	{
		public void ProcessRequest(HttpContext context) {
			//throw new NotImplementedException();
		}

		public bool IsReusable {
			get {
				return false;
			}
		}
	}

	
}
