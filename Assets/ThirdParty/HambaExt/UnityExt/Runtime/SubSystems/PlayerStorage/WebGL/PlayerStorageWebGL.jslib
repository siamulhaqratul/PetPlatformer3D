// WebGL bridge for SaveDataWriteOnDevice.
// Calls SaveDataWriteOnDevice.OnRefresh on the "__STORAGE__WRITER__" GameObject
// (UConfig.PlayerStorageGameObjectName) when the page is refreshed or closed.
mergeInto(LibraryManager.library, {
  RegisterBeforeUnloadWebGL: function () {
    if (Module.__storageBeforeUnloadRegistered) return;
    Module.__storageBeforeUnloadRegistered = true;

    window.addEventListener('beforeunload', function () {
      var send = (typeof SendMessage === 'function') ? SendMessage : Module.SendMessage;
      if (send) {
        send('__STORAGE__WRITER__', 'OnRefresh');
      }
    });
  }
});
