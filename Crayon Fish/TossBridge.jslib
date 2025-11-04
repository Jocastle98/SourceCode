var BridgePlugin = {
  ExecuteJavaScriptMethod: function (methodPtr) {
    var jsMethod = UTF8ToString(methodPtr);
    try {
      eval(jsMethod);
    } catch (error) {
      console.error('JavaScript Error : ', error);
    }
  }
};
mergeInto(LibraryManager.library, BridgePlugin);
