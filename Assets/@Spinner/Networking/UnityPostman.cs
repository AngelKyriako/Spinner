using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;

using ServerError = System.String;

public class UnityPostman : IPostman {

    public event Action<string, string> OnRequestSent;
    public event Action<string, string> OnResponseSucceed;
    public event Action<string, string> OnResponseFailed;

    private ISerializer _serializer;
    private ILogger _logger;
    private string _serverUrl;
    private int _timeoutInSeconds;

    private string _bearer;

    private MonoBehaviour _routineInvocator;

    public UnityPostman(MonoBehaviour routineInvocator, Uri serverUrl, ILogger logger, ISerializer serializer, int defaultTimeoutInSeconds = 10) {
        _routineInvocator = routineInvocator;
        _logger = logger;
        _serializer = serializer;
        _serverUrl = serverUrl.ToString();
        _timeoutInSeconds = defaultTimeoutInSeconds;
    }

    // ----------------------------------------------------------------------
    // REGION API
    public IPostman SetAuthorizationBearer(string token) {
        if (token != null) {
            _bearer = "Bearer " + token;
        } else {
            _bearer = null;
        }

        return this;
    }

    public Task<M> Get<M>(string route) where M : class {
        TaskAPIPrecondition();
        return SendAsync<object, M>(route, UnityWebRequest.kHttpVerbGET);
    }

    public Task<IEnumerable<M>> GetMany<M>(string route) where M : class {
        TaskAPIPrecondition();
        return SendAsync<object, IEnumerable<M>>(route, UnityWebRequest.kHttpVerbGET);
    }

    public Task<RES_M> Post<REQ_M, RES_M>(string route, REQ_M model) where REQ_M : class where RES_M : class {
        TaskAPIPrecondition();
        return SendAsync<REQ_M, RES_M>(route, UnityWebRequest.kHttpVerbPOST, model);
    }

    public Task<RES_M> Put<REQ_M, RES_M>(string route, REQ_M model) where REQ_M : class where RES_M : class {
        TaskAPIPrecondition();
        return SendAsync<REQ_M, RES_M>(route, UnityWebRequest.kHttpVerbPUT, model);
    }

    public Task<M> Delete<M>(string route) where M : class {
        TaskAPIPrecondition();
        return SendAsync<object, M>(route, UnityWebRequest.kHttpVerbDELETE);
    }

    private void TaskAPIPrecondition() {
        if (Application.platform == UnityEngine.RuntimePlatform.WebGLPlayer) {
            throw new InvalidOperationException("Web platforms do not support multithreaded operations, use the callback API instead.");
        }
    }

    public void Get<M>(string route, Action<ServerError, M> onDone) where M : class {
        _routineInvocator.StartCoroutine(SendRoutine<object, M>(route, UnityWebRequest.kHttpVerbGET, null, onDone));
    }

    public void GetMany<M>(string route, Action<ServerError, IEnumerable<M>> onDone) where M : class {
        _routineInvocator.StartCoroutine(SendRoutine<object, IEnumerable<M>>(route, UnityWebRequest.kHttpVerbGET, null, onDone));
    }

    public void Post<REQ_M, RES_M>(string route, REQ_M model, Action<ServerError, RES_M> onDone)
        where REQ_M : class
        where RES_M : class {
        _routineInvocator.StartCoroutine(SendRoutine(route, UnityWebRequest.kHttpVerbPOST, model, onDone));
    }

    public void Put<REQ_M, RES_M>(string route, REQ_M model, Action<ServerError, RES_M> onDone)
        where REQ_M : class
        where RES_M : class {
        _routineInvocator.StartCoroutine(SendRoutine(route, UnityWebRequest.kHttpVerbPUT, model, onDone));
    }

    public void Delete<M>(string route, Action<ServerError, M> onDone) where M : class {
        _routineInvocator.StartCoroutine(SendRoutine<object, M>(route, UnityWebRequest.kHttpVerbDELETE, null, onDone));
    }

    public void Send<M>(string route, string verb, Action<ServerError, M> onDone) where M : class {
        _routineInvocator.StartCoroutine(SendRoutine<object, M>(route, verb, null, onDone));
    }
    // REGION API END
    // ----------------------------------------------------------------------

    /// <summary>
    /// Transforms the SendRoutine IEnumerator to a Task that can be awaited without thread usage.
    /// That way we ensure the UnityPostman can run in web platforms which are single threaded.
    /// </summary>
    private Task<RES_M> SendAsync<REQ_M, RES_M>(string route, string method, REQ_M reqBody = null) where REQ_M : class where RES_M : class {
        TaskCompletionSource<RES_M> taskSource = new TaskCompletionSource<RES_M>();

        _routineInvocator.StartCoroutine(SendRoutine<REQ_M, RES_M>(route, method, reqBody, (err, res) => {
            if (err != null) {
                taskSource.SetException(new InvalidOperationException(err));
            } else if (res != null) {
                taskSource.SetResult(res);
            } else {
                taskSource.SetCanceled();
            }
        }));

        return taskSource.Task;
    }

    /// <summary>
    /// Handles the flow of an http request via UnityWebRequest class.
    /// HINT: https://codetolight.wordpress.com/2017/01/18/unity-rest-api-interaction-with-unitywebrequest-and-besthttp/
    /// </summary>
    private IEnumerator SendRoutine<REQ_M, RES_M>(
        string route,
        string method,
        REQ_M reqBody,
        Action<ServerError, RES_M> onDone
    ) where REQ_M : class where RES_M : class {
        // ----------------------------------------------------------------------
        // REGION Request
        UnityWebRequest req = new UnityWebRequest();

        string reqBodyJson = reqBody != null ? _serializer.Serialize(reqBody) : string.Empty;
        _logger.LogFormat(LogType.Log, "Sending {0} at {1} with payload: {2}", method, route, reqBodyJson.Length > 0 ? reqBodyJson : "NONE");

        OnRequestSent?.Invoke(method, route);

        req.url = Path.Combine(_serverUrl, route);
        req.method = method;
        if (reqBodyJson.Length > 0) {
            byte[] reqBodyBytes = new System.Text.UTF8Encoding().GetBytes(reqBodyJson);
            req.uploadHandler = new UploadHandlerRaw(reqBodyBytes);
        }
        req.downloadHandler = new DownloadHandlerBuffer();

        req.SetRequestHeader("Content-Type", "application/json");
        if (_bearer != null) {
            req.SetRequestHeader("Authorization", _bearer);
        }

        req.timeout = _timeoutInSeconds;

        yield return req.SendWebRequest();
        // REGION Request END
        // ----------------------------------------------------------------------

        // ----------------------------------------------------------------------
        // REGION Response
        string resStatusCode = req.responseCode.ToString();
        string resBodyJson = req.downloadHandler.text;

        _logger.LogFormat(LogType.Log, "handling response: status: {0}, body: {1}, isNetErr: {2}", resStatusCode, resBodyJson, req.isNetworkError);

        if (req.isNetworkError) {
            OnResponseFailed?.Invoke("NONE", req.error);

            onDone("Network error", null);
        } else {
            ServerError err = null;
            RES_M data = null;

            try {
                if (!req.isHttpError)
                    data = _serializer.Deserialize<RES_M>(resBodyJson);
                else
                    err = _serializer.Deserialize<ServerError>(resBodyJson);
            } catch (Exception e) {
                _logger.LogFormat(LogType.Warning, "Exception was throwed during response deserialization: {0}", e);
            }

            if (data == null && err == null) {
                err = string.Format("Failed to deserialize data for response with status: {0}.", resStatusCode);
            }

            if (err != null) {
                _logger.Log("response is faulted.");

                OnResponseFailed?.Invoke(resStatusCode, resBodyJson);
                onDone(err, null);
            } else {
                _logger.Log("response is successful.");

                OnResponseSucceed?.Invoke(resStatusCode, resBodyJson);
                onDone(null, _serializer.Deserialize<RES_M>(resBodyJson));
            }
        }
        // REGION Response END
        // ----------------------------------------------------------------------
    }
}
