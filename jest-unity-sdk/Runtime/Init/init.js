function marshalString(value) {
  var bufferSize = lengthBytesUTF8(value) + 1;
  var buffer = _malloc(bufferSize);
  stringToUTF8(value, buffer, bufferSize);
  return buffer;
}

async function initializeSdkAsync() {
  await new Promise((resolve, reject) => {
    const script = document.createElement('script');
    script.src = 'https://cdn.jest.com/sdk/latest/jestsdk.js';
    script.onload = resolve;
    script.onerror = reject;
    document.head.appendChild(script);
  });
  await window.JestSDK.init();
  console.log("Initialized.");
}

async function bootstrapSdkAsync(initializeSdkAsync) {
  try {
    // Do not start engine main() until we have downloaded & initialized the SDK
    Module.addRunDependency("initializeSdkAsync");
    await initializeSdkAsync();
  } catch (e) {
    console.error(`Unable to initialize SDK - ${e.message}`);
  } finally {
    Module.removeRunDependency("initializeSdkAsync");
  }
}

// Only run this code in the main thread
if (!Module.ENVIRONMENT_IS_PTHREAD) {
  Module.preRun.push(() => bootstrapSdkAsync(initializeSdkAsync));
}
