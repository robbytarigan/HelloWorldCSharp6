using System;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.InteropServices;
using Xunit;

namespace Span
{
    public class SpanExamples
    {
        [Fact]
        public void CreateASpanFromArray()
        {
		var arr = new byte[10];
		Span<byte> bytes = arr; // Implicit cast from T[] to Span<T>
        }

		[Fact]
		public void Represent_just_a_subset_of_array() {
			var arr = new byte[10];
			Span<byte> bytes = arr; // Implicit cast from T[] to Span<T>

			Span<byte> slicedBytes = bytes.Slice(start: 5, length: 2);
			slicedBytes[0] = 42;
			slicedBytes[1] = 43;
			Assert.Equal(42, slicedBytes[0]);
			Assert.Equal(43, slicedBytes[1]);
			Assert.Equal(arr[5], slicedBytes[0]);
			Assert.Equal(arr[6], slicedBytes[1]);
			// slicedBytes[2] = 44; // Throws IndexOutOfRangeException
			bytes[2] = 45; // OK
			Assert.Equal(arr[2], bytes[2]);
			Assert.Equal(45, arr[2]);
		}

		[Fact]
		public void Refer_data_on_the_stack() {
			Span<byte> bytes = stackalloc byte[2]; 	// Using C# 7.2 stackalloc support for spans
			bytes[0] = 42;
			bytes[1] = 43;
			Assert.Equal(42, bytes[0]);
			Assert.Equal(43, bytes[1]);
			// bytes[2] = 44; // Throws indexOutOfRangeException
		}

		[Fact]
		public void Refer_to_arbitrary_pointers_and_lengths() {
			IntPtr ptr = Marshal.AllocHGlobal(1);
			try {
				Span<byte> bytes;
				unsafe { bytes = new Span<byte>((byte*)ptr, 1); }
				bytes[0] = 42;
				Assert.Equal(42, bytes[0]);
				Assert.Equal(Marshal.ReadByte(ptr), bytes[0]);
				//bytes[1] = 43; // Throws IndexOutOfRangeException
			} 
			finally {
				Marshal.FreeHGlobal(ptr);
			}
		}

		[Fact]
		public void Compare_with_list_of_T_indexer() 	{

			Span<MutableStruct> spanOfStructs = new MutableStruct[1];
			spanOfStructs[0].Value = 42;
			Assert.Equal(42, spanOfStructs[0].Value);
			var listOfStructs = new List<MutableStruct> { new MutableStruct() };
			// listOfStructs[0].Value = 42; // Error CS1612: the return value is not a variable
		}

		struct MutableStruct { public int Value; }

		[Fact]
		public void ReadOnlySpan_enables_read_only_access() {
			string str = "hello, world";
			string worldString = str.Substring(startIndex: 7, length: 5); // Allocates
			ReadOnlySpan<char> worldSpan = str.AsReadOnlySpan().Slice(start: 7, length: 5); // No allocation
			Assert.Equal('w', worldSpan[0]);
			// worldSpan[0] = 'a';	// Error CS20200: indexer cannot be assigned to
		}

		[Fact]
		public void Refer_the_middle_of_objects_like_arrays()	{
			var arr = new byte[100];
			Span<byte> interiorRef1 = arr.AsSpan().Slice(start: 20);
			Span<byte> interiorRef2 = new Span<byte>(arr, 20, arr.Length - 20);
			//Span<byte> interiorRef3 = Span<byte>.DangerousCreate(arr, ref arr[20], arr.Length - 20);
		}
    }
}
