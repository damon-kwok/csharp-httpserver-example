<a href="http://planet.lisp.org"><img src="https://dhalgara.com/content/images/2018/07/dotnet-logo.png" alt="dotnet Logo" width="80" height="80" align="right"></a>
[![Rider](https://img.shields.io/badge/IDE-Rider-1f425f.svg)](https://www.jetbrains.com/rider/)
[![GitHub license](https://img.shields.io/badge/license-BSD%202%20Clause-2e8b57.svg)](https://github.com/damon-kwok/csharp-httpserver-example/blob/main/LICENSE)

# CSharp HttpServer example

## TODO

- [x] Base: Easy `RESTfulService` library
- [x] Base: `SkipList` based rank
- [x] Base: `Regex based` route parse
- [x] Style: Editorconfig file
- [x] Base: Key code comments
- [ ] Doc: doxygen support
- [ ] Base: Testcases
- [ ] Performance: Performance profiler
- [ ] Library: Route `real data type` parser
- [ ] Extend: `HTTPS` and `Certificate` support
- [ ] Extend: More `Status Code` support
- [ ] Extend: `Binary response` support
- [ ] Performance: Replacing the `queue` with the `Lock-free queue`
- [ ] Performance: Better caches for `Rank`
- [ ] Performance: Replacing the `lock` with `spinlock/rwlock/seqlock`
- [ ] Performance: `RCU` for `List`
- [ ] Performance: CPU core binding
- [ ] Script(Linux): Make the OS CPU core isolation(use NUMA 1)
- [ ] Script(Linux): Make the program work on the specified NUMA(2 ... N)
- [ ] Style: Swagger style REST API
- [ ] Style: MVC or MVVM support
- [ ] Style: Microservices
- [ ] Safe: HTTP(s) slow connection defense
- [ ] Extend: Better HTTP(s) library, use `epoll/io_uring/kqueue/IOCP`
- [ ] Deployment: Docker/K8S support
- [ ] Build: MSBuild support
- [ ] Performance: Replacing the default NIC driver with `INTEL DPDK`

## Known Issues

- Viewer: No data paging
- The current skiplist is not idempotent