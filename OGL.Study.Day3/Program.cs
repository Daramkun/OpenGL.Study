using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OGL.Study.Day3
{
	static class Program
	{
		[STAThread]
		static void Main ()
		{
			// OpenGL 창
			GameWindow window = new GameWindow ();

			int vertexBuffer = 0;

			// 창이 처음 생성됐을 때 
			window.Load += ( sender, e ) =>
			{
				// 정점 버퍼 생성
				vertexBuffer = GL.GenBuffer ();
				// 정점 버퍼 입력
				GL.BindBuffer ( BufferTarget.ArrayBuffer, vertexBuffer );

				// 정점 버퍼에 정점 데이터 입력
				float [] vertices =
				{
					+0.0f, +0.5f,
					+0.5f, -0.5f,
					-0.5f, -0.5f,
				};
				GL.BufferData<float> ( BufferTarget.ArrayBuffer, new IntPtr ( vertices.Length * sizeof ( float ) ), vertices, BufferUsageHint.StaticDraw );
			};
			// 업데이트 프레임(연산처리, 입력처리 등)
			window.UpdateFrame += ( sender, e ) =>
			{

			};
			// 렌더링 프레임(화면 표시)
			window.RenderFrame += ( sender, e ) =>
			{
				// 화면 초기화 설정
				//> 화면 색상은 검정색(R: 0, G: 0, B: 0, A: 255)
				GL.ClearColor ( 0, 0, 0, 1 );
				//> 깊이 버퍼는 1(쓸 수 있는 깊이)
				GL.ClearDepth ( 1 );
				//> 스텐실 버퍼는 0
				GL.ClearStencil ( 0 );
				// 화면 초기화(색상 버퍼, 깊이 버퍼, 스텐실 버퍼에 처리)
				GL.Clear ( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit );

				// 정점 버퍼 입력(쉐이더를 사용하지 않는 방법으로)
				// OpenGL 3.2부터 Deprecated, OpenGL ES 2.0부터 Deprecated
				GL.BindBuffer ( BufferTarget.ArrayBuffer, vertexBuffer );
				GL.VertexPointer ( 2, VertexPointerType.Float, 0, 0 );
				GL.EnableClientState ( ArrayCap.VertexArray );

				// 삼각형 색상은 마젠타(R: 255, G: 0, B: 255)
				// OpenGL 3.2부터 Deprecated, OpenGL ES 2.0부터 Deprecated
				GL.Color3 ( 1.0f, 0, 1.0f );

				GL.DrawArrays ( PrimitiveType.Triangles, 0, 3 );

				// 백 버퍼와 화면 버퍼 교환
				window.SwapBuffers ();
			};
			// 창이 종료될 때
			window.Closing += ( sender, e ) =>
			{
				// 정점 버퍼 제거
				//> 이 과정을 처리하지 않으면 비디오 메모리 누수가 발생할 수 있음
				GL.DeleteBuffer ( vertexBuffer );
			};

			// 창을 띄우고 창이 종료될 때까지 메시지 펌프 및 무한 루프 처리
			window.Run ();
		}
	}
}
