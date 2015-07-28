using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OGL.Study.Day1
{
	static class Program
	{
		[STAThread]
		static void Main ()
		{
			// OpenGL 창
			GameWindow window = new GameWindow ();
			
			// 창이 처음 생성됐을 때 
			window.Load += ( sender, e ) =>
			{
				
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

				// OpenGL 1.0 삼각형 그리기 시작
				// OpenGL 3.2부터 Deprecated, OpenGL ES 2.0부터 Deprecated
				GL.Begin ( PrimitiveType.Triangles );

				// 삼각형의 세 꼭지점 설정
				// OpenGL 3.2부터 Deprecated, OpenGL ES 2.0부터 Deprecated
				GL.Vertex2 ( +0.0, +0.5 );
				GL.Vertex2 ( +0.5, -0.5 );
				GL.Vertex2 ( -0.5, -0.5 );

				// 삼각형 색상은 마젠타(R: 255, G: 0, B: 255)
				// OpenGL 3.2부터 Deprecated, OpenGL ES 2.0부터 Deprecated
				GL.Color3 ( 1.0f, 0, 1.0f );

				// OpenGL 1.0 삼각형 그리기 끝
				// OpenGL 3.2부터 Deprecated, OpenGL ES 2.0부터 Deprecated
				GL.End ();
				// 버퍼에 출력
				GL.Flush ();

				// 백 버퍼와 화면 버퍼 교환
				window.SwapBuffers ();
			};
			// 창이 종료될 때
			window.Closing += ( sender, e ) =>
			{
				
			};

			// 창을 띄우고 창이 종료될 때까지 메시지 펌프 및 무한 루프 처리
			window.Run ();
		}
	}
}
