using System;
using System.Threading;
using NUnit.Framework;
using OpenRasta.Diagnostics;
using OpenRasta.Pipeline;
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
							errorCount = errorCount+1;
							throw;
						}

					}).Start();
				}

				Assert.That(errorCount, Is.EqualTo(0), "Some of the requests threw errors meaning that the dependencyresolver is not threadsafe ");
			}
		}
	
}
