let recording = null;
let stream = null;

const sampleRate = 16000;
const gainValue = 1;

export function isRecording() {
    return recording !== null;
}

export async function initialize() {
    if (stream != null) return;
    if (navigator.mediaDevices == null) {
        alert("Please allow to use microphone.")
        return;
    }
    stream = await navigator.mediaDevices.getUserMedia({
        audio: {
            channelCount: 1,
            sampleRate: sampleRate,
        },
        video: false
    });
}

export async function startRecording(backend) {
    if (isRecording())
        return null;
    await initialize()

    // Creating PCM encode pipeline
    let AudioContext = window.AudioContext || window.webkitAudioContext;
    let context = new AudioContext({
        latencyHint: 'interactive',
        sampleRate: sampleRate,
        channelCount: 1,
    });
    let sourceNode = context.createMediaStreamSource(stream);
    let gainNode = context.createGain();
    gainNode.gain.setValueAtTime(gainValue, context.currentTime);
    let finalNode = (context.createScriptProcessor || context.createJavaScriptNode).call(context, 4096, 1, 1);
    finalNode.onaudioprocess = e => {
        if (!isRecording()) return;
        let sample = e.inputBuffer.getChannelData(0);
        enqueueSample(sample, recording.sampleIndex);
        recording.sampleIndex++;
    };

    recording = {
        backend: backend,
        context: context,
        stream: stream,
        sampleIndex: 0,
        lastSendPromise: null,
    };

    sourceNode.connect(gainNode);
    gainNode.connect(finalNode);
    finalNode.connect(context.destination);
    return recording;
}

export async function stopRecording() {
    if (!isRecording()) return;
    let r = recording;
    recording = null;
    await r.context.close();
    await r.backend.invokeMethodAsync('RecordingStoppedAsync');
}

async function enqueueSample(sample, sampleIndex) {
    if (!isRecording()) return;
    let r = recording;
    let lastSendPromise = recording.lastSendPromise;
    recording.sendPromise = (async () => {
        if (lastSendPromise !== null)
            await lastSendPromise;
        if (sampleIndex === 0) {
            if (!isRecording()) return;
            await r.backend.invokeMethodAsync('RecordingStartedAsync');
        }
        if (!isRecording()) return;
        // let data = bufferToBase64(encodeWav(sample, sampleIndex === 0));
        let data = bufferToBase64(encodeWav(sample, false));
        await r.backend.invokeMethodAsync('RecordingDataAvailableAsync', data);
    })();
}

function bufferToBase64(buffer) {
    let binary = '';
    const bytes = new Uint8Array(buffer);
    const len = bytes.byteLength;
    for (let i = 0; i < len; i++)
        binary += String.fromCharCode(bytes[i]);
    return window.btoa(binary);
}

function encodeWav(samples, addHeader) {
    let headerLength = addHeader ? 44 : 0;
    let buffer = new ArrayBuffer(headerLength + samples.length * 2);
    let view = new DataView(buffer);

    if (addHeader) {
        /* RIFF identifier */
        writeString(view, 0, 'RIFF');
        /* RIFF chunk length */
        view.setUint32(4, 36 + samples.length * 2, true);
        /* RIFF type */
        writeString(view, 8, 'WAVE');
        /* format chunk identifier */
        writeString(view, 12, 'fmt ');
        /* format chunk length */
        view.setUint32(16, 16, true);
        /* sample format (raw) */
        view.setUint16(20, 1, true);
        /* channel count */
        view.setUint16(22, 1, true);
        /* sample rate */
        view.setUint32(24, sampleRate, true);
        /* byte rate (sample rate * block align) */
        view.setUint32(28, sampleRate * 4, true);
        /* block align (channel count * bytes per sample) */
        view.setUint16(32, 2, true);
        /* bits per sample */
        view.setUint16(34, 16, true);
        /* data chunk identifier */
        writeString(view, 36, 'data');
        /* data chunk length */
        view.setUint32(40, samples.length * 2, true);
    }

    pcmEncodeSamples(view, headerLength, samples);
    return buffer;
}

function writeString(view, offset, value) {
    for (let i = 0; i < value.length; i++)
        view.setUint8(offset + i, value.charCodeAt(i));
}

function pcmEncodeSamples(view, offset, samples) {
    for (let i = 0; i < samples.length; i++, offset += 2) {
        let v = 0x7FFF * Math.max(-1, Math.min(1, samples[i]));
        view.setInt16(offset, v, true);
    }
}
