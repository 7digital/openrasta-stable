using System.Threading.Tasks;
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
				Parallel.For(0, 1000,
					i => {
						var container = new Container();
						container.Configure(c => c.Scan(s =>{
							s.AssemblyContainingType(GetType());
							s.LookForRegistries();
							s.WithDefaultConventions();
						}));

						_resolver = new StructureMapDependencyResolver(container);
						_resolver.AddDependency(typeof(IDependencyResolver), typeof(StructureMapDependencyResolver), DependencyLifetime.Singleton);
						_resolver.AddDependency(typeof(IPipeline), typeof(PipelineRunner), DependencyLifetime.Singleton);
						_resolver.AddDependency(typeof(ILogger), typeof(NullLogger), DependencyLifetime.Singleton);

						var pipeline = _resolver.Resolve<IPipeline>();
						Assert.That(pipeline, Is.Not.Null);
					}
				);
			}

		}
	
}
