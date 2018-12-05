using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ServerError = System.String;

public interface IPostman {
    event Action<string, string> OnRequestSent;
    event Action<string, string> OnResponseSucceed;
    event Action<string, string> OnResponseFailed;

    IPostman SetAuthorizationBearer(string token);

    // REGION Task API
    Task<M> Get<M>(string route) where M : class;
    Task<IEnumerable<M>> GetMany<M>(string route) where M : class;

    Task<RES_M> Post<REQ_M, RES_M>(string route, REQ_M model)
        where REQ_M : class
        where RES_M : class;

    Task<RES_M> Put<REQ_M, RES_M>(string route, REQ_M model)
        where REQ_M : class
        where RES_M : class;

    Task<M> Delete<M>(string route) where M : class;
    // REGION Task API END

    // REGION Callback API
    void Get<M>(string route, Action<ServerError, M> onDone) where M : class;

    void GetMany<M>(string route, Action<ServerError, IEnumerable<M>> onDone) where M : class;

    void Post<REQ_M, RES_M>(string route, REQ_M model, Action<ServerError, RES_M> onDone)
        where REQ_M : class
        where RES_M : class;

    void Put<REQ_M, RES_M>(string route, REQ_M model, Action<ServerError, RES_M> onDone)
        where REQ_M : class
        where RES_M : class;

    void Delete<M>(string route, Action<ServerError, M> onDone) where M : class;

    void Send<M>(string route, string verb, Action<ServerError, M> onDone) where M : class;
    // REGION Callback API END
}

public static class IPostmanExtensions {

    public static IPostman CleanAuthorizationBearer(this IPostman postman) {
        return postman.SetAuthorizationBearer(null);
    }

    public static Task<M> Post<M>(this IPostman postman, string route, M model) where M : class {
        return postman.Post<M, M>(route, model);
    }

    public static Task<M> Put<M>(this IPostman postman, string route, M model) where M : class {
        return postman.Put<M, M>(route, model);
    }

    public static void Post<M>(this IPostman postman, string route, M model, Action<ServerError, M> onDone) where M : class {
        postman.Post<M, M>(route, model, onDone);
    }

    public static void Put<M>(this IPostman postman, string route, M model, Action<ServerError, M> onDone) where M : class {
        postman.Put<M, M>(route, model, onDone);
    }

    public static void Send<M>(this IPostman postman, RouteConfiguration route, Action<ServerError, M> onDone) where M : class {
        postman.Send(route.Path, route.Verb, onDone);
    }
}
