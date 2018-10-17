using System;
using Android.Runtime;
using ApxLabs.FastAndroidCamera;
using Java.Nio;

namespace Steepshot.CameraGL
{
    public static class ByteBufferExtensions
    {
        private static IntPtr _byteBufferClassRef;
        private static IntPtr _byteBufferWrapBi;
        private static IntPtr _byteBufferGetBii;
        private static IntPtr _byteBufferPutBii;
        private static IntPtr _byteBufferPutB;

        public static ByteBuffer Wrap(FastJavaByteArray dst)
        {
            if (_byteBufferClassRef == IntPtr.Zero)
            {
                _byteBufferClassRef = JNIEnv.FindClass("java/nio/ByteBuffer");
            }
            if (_byteBufferWrapBi == IntPtr.Zero)
            {
                _byteBufferWrapBi = JNIEnv.GetStaticMethodID(_byteBufferClassRef, "wrap", "([B)Ljava/nio/ByteBuffer;");
            }

            return Java.Lang.Object.GetObject<ByteBuffer>(JNIEnv.CallStaticObjectMethod(_byteBufferClassRef, _byteBufferWrapBi, new JValue(dst.Handle)), JniHandleOwnership.TransferLocalRef);
        }

        public static ByteBuffer Get(this ByteBuffer buffer, FastJavaByteArray dst, int dstOffset, int byteCount)
        {
            if (_byteBufferClassRef == IntPtr.Zero)
            {
                _byteBufferClassRef = JNIEnv.FindClass("java/nio/ByteBuffer");
            }
            if (_byteBufferGetBii == IntPtr.Zero)
            {
                _byteBufferGetBii = JNIEnv.GetMethodID(_byteBufferClassRef, "get", "([BII)Ljava/nio/ByteBuffer;");
            }

            return Java.Lang.Object.GetObject<ByteBuffer>(JNIEnv.CallObjectMethod(buffer.Handle, _byteBufferGetBii, new JValue(dst.Handle), new JValue(dstOffset), new JValue(byteCount)), JniHandleOwnership.TransferLocalRef);
        }

        public static ByteBuffer Put(this ByteBuffer buffer, FastJavaByteArray dst, int dstOffset, int byteCount)
        {
            if (_byteBufferClassRef == IntPtr.Zero)
            {
                _byteBufferClassRef = JNIEnv.FindClass("java/nio/ByteBuffer");
            }
            if (_byteBufferPutBii == IntPtr.Zero)
            {
                _byteBufferPutBii = JNIEnv.GetMethodID(_byteBufferClassRef, "put", "([BII)Ljava/nio/ByteBuffer;");
            }

            return Java.Lang.Object.GetObject<ByteBuffer>(JNIEnv.CallObjectMethod(buffer.Handle, _byteBufferPutBii, new JValue(dst.Handle), new JValue(dstOffset), new JValue(byteCount)), JniHandleOwnership.TransferLocalRef);
        }
    }
}