﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OGL.Study.DayB
{
	static class Program
	{
		[STAThread]
		static void Main ()
		{
			// OpenGL 창
			GameWindow window = new GameWindow ( 800, 600, new OpenTK.Graphics.GraphicsMode ( new OpenTK.Graphics.ColorFormat ( 32 ), 16, 8 ) );

			int vertexBuffer = 0, vertexBuffer2 = 0;
			int vertexShader = 0, fragmentShader = 0, programId = 0;
			int frameBuffer = 0, texture = 0;
			int vertexShader2 = 0, fragmentShader2 = 0, programId2 = 0;

			float rotationAngle = 0;

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
					-0.5f, -0.5f, -0.5f,
					 0.5f, -0.5f, -0.5f,
					 0.5f,  0.5f, -0.5f,
					 0.5f,  0.5f, -0.5f,
					-0.5f,  0.5f, -0.5f,
					-0.5f, -0.5f, -0.5f,

					-0.5f, -0.5f,  0.5f,
					 0.5f, -0.5f,  0.5f,
					 0.5f,  0.5f,  0.5f,
					 0.5f,  0.5f,  0.5f,
					-0.5f,  0.5f,  0.5f,
					-0.5f, -0.5f,  0.5f,

					-0.5f,  0.5f,  0.5f,
					-0.5f,  0.5f, -0.5f,
					-0.5f, -0.5f, -0.5f,
					-0.5f, -0.5f, -0.5f,
					-0.5f, -0.5f,  0.5f,
					-0.5f,  0.5f,  0.5f,

					 0.5f,  0.5f,  0.5f,
					 0.5f,  0.5f, -0.5f,
					 0.5f, -0.5f, -0.5f,
					 0.5f, -0.5f, -0.5f,
					 0.5f, -0.5f,  0.5f,
					 0.5f,  0.5f,  0.5f,

					-0.5f, -0.5f, -0.5f,
					 0.5f, -0.5f, -0.5f,
					 0.5f, -0.5f,  0.5f,
					 0.5f, -0.5f,  0.5f,
					-0.5f, -0.5f,  0.5f,
					-0.5f, -0.5f, -0.5f,

					-0.5f,  0.5f, -0.5f,
					 0.5f,  0.5f, -0.5f,
					 0.5f,  0.5f,  0.5f,
					 0.5f,  0.5f,  0.5f,
					-0.5f,  0.5f,  0.5f,
					-0.5f,  0.5f, -0.5f
				};
				GL.BufferData<float> ( BufferTarget.ArrayBuffer, new IntPtr ( vertices.Length * sizeof ( float ) ), vertices, BufferUsageHint.StaticDraw );
				
				// 쉐이더 생성
				vertexShader = GL.CreateShader ( ShaderType.VertexShader );
				fragmentShader = GL.CreateShader ( ShaderType.FragmentShader );

				// 컴파일 할 소스 입력
				//> GLSL 요구 버전은 OpenGL 3.2 (GLSL 1.5)
				GL.ShaderSource ( vertexShader, @"#version 150
// 정점 쉐이더 입력 인자는 3차원 위치 벡터 하나
in vec3 in_pos;

// 변환 행렬을 가져옴
uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main () {
	// 정점 위치 설정
	//> vec3를 vec4로 변환한 이유는 아핀 공간(Affine space)에 맞추기 위해서
	gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4 ( in_pos, 1 );
}" );
				GL.ShaderSource ( fragmentShader, @"#version 150
uniform vec4 outColor;

void main () {
	gl_FragColor = outColor;
}" );
				// 쉐이더 소스 컴파일
				GL.CompileShader ( vertexShader );
				GL.CompileShader ( fragmentShader );

				// 쉐이더 프로그램 생성 및 쉐이더 추가
				programId = GL.CreateProgram ();
				GL.AttachShader ( programId, vertexShader );
				GL.AttachShader ( programId, fragmentShader );

				// 쉐이더 프로그램에 각 쉐이더 링크
				GL.LinkProgram ( programId );

				// 프레임버퍼 출력용 쉐이더 생성
				vertexShader2 = GL.CreateShader ( ShaderType.VertexShader );
				fragmentShader2 = GL.CreateShader ( ShaderType.FragmentShader );

				// 컴파일 할 소스 입력
				//> GLSL 요구 버전은 OpenGL 3.2 (GLSL 1.5)
				GL.ShaderSource ( vertexShader2, @"#version 150
// 정점 쉐이더 입력 인자는 2차원 위치 벡터 둘
in vec2 in_pos;
in vec2 in_tex;

out vec2 texcoord;

// 변환 행렬을 가져옴
uniform mat4 projectionMatrix;

void main () {
	// 정점 위치 설정
	//> vec2를 vec4로 변환한 이유는 아핀 공간(Affine space)에 맞추기 위해서
	gl_Position = projectionMatrix * vec4 ( in_pos, 0, 1 );
	texcoord = in_tex;
}" );
				GL.ShaderSource ( fragmentShader2, @"#version 150
in vec2 texcoord;

uniform sampler2D fb;

void main () {
	// 프레임버퍼 텍스처의 색상과 좌표를 대강 조합하여 만들어낸 색상 값을 픽셀 쉐이더의 결과로 제출
	gl_FragColor = texture ( fb, texcoord ) * 
		vec4 ( texcoord.x, texcoord.y, texcoord.x + texcoord.y, 1 ) +
		vec4 ( texcoord.x + texcoord.y, texcoord.y, texcoord.x, 1 );
}" );
				// 쉐이더 소스 컴파일
				GL.CompileShader ( vertexShader2 );
				GL.CompileShader ( fragmentShader2 );

				// 쉐이더 프로그램 생성 및 쉐이더 추가
				programId2 = GL.CreateProgram ();
				GL.AttachShader ( programId2, vertexShader2 );
				GL.AttachShader ( programId2, fragmentShader2 );

				// 쉐이더 프로그램에 각 쉐이더 링크
				GL.LinkProgram ( programId2 );

				// 프레임버퍼에 사용할 텍스처 생성
				texture = GL.GenTexture ();
				GL.BindTexture ( TextureTarget.Texture2D, texture );
				GL.TexImage2D ( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, 800, 600, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero );
				GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, ( int ) TextureMinFilter.Linear );
				GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, ( int ) TextureMinFilter.Linear );

				// 프레임버퍼 생성
				frameBuffer = GL.GenFramebuffer ();
				
				GL.BindFramebuffer ( FramebufferTarget.Framebuffer, frameBuffer );
				
				// 프레임버퍼에 텍스처 부착
				GL.FramebufferTexture2D ( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture, 0 );
				
				// 정점 버퍼 생성
				vertexBuffer2 = GL.GenBuffer ();
				// 정점 버퍼 입력
				GL.BindBuffer ( BufferTarget.ArrayBuffer, vertexBuffer2 );

				// 정점 버퍼에 정점 데이터 입력
				float [] vertices2 =
				{
					  0.0f,   0.0f, 0.0f, 1.0f,
					  0.0f, 600.0f, 0.0f, 0.0f,
					800.0f,   0.0f, 1.0f, 1.0f,

					800.0f, 600.0f, 1.0f, 0.0f,
					  0.0f, 600.0f, 0.0f, 0.0f,
                    800.0f,   0.0f, 1.0f, 1.0f,
				};
				GL.BufferData<float> ( BufferTarget.ArrayBuffer, new IntPtr ( vertices2.Length * sizeof ( float ) ), vertices2, BufferUsageHint.StaticDraw );
			};
			// 업데이트 프레임(연산처리, 입력처리 등)
			window.UpdateFrame += ( sender, e ) =>
			{
				rotationAngle += ( float ) e.Time;
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
				// 화면 초기화(색상 버퍼, 깊이 버퍼에 처리)
				GL.Clear ( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

				// 프레임버퍼 적용
				GL.BindFramebuffer ( FramebufferTarget.Framebuffer, frameBuffer );
				
				// 깊이 테스트 켬
				GL.Enable ( EnableCap.DepthTest );

				// 프레임버퍼에 대한 뷰포트 지정
				GL.Viewport ( 0, 0, 800, 600 );

				// 프레임버퍼 초기화 설정
				//> 화면 색상은 검정색(R: 0, G: 0, B: 0, A: 255)
				GL.ClearColor ( 0, 0, 0, 1 );
				//> 깊이 버퍼는 1(쓸 수 있는 깊이)
				GL.ClearDepth ( 1 );
				//> 스텐실 버퍼는 0
				GL.ClearStencil ( 0 );
				// 화면 초기화(색상 버퍼, 깊이 버퍼에 처리)
				GL.Clear ( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

				// 상자
				DrawObject ( programId, vertexBuffer, 36, Matrix4.CreateRotationY ( rotationAngle ), new Vector4 ( 1, 1, 1, 1 ) );
				DrawObject ( programId, vertexBuffer, 36, Matrix4.CreateRotationX ( rotationAngle ) * 
					Matrix4.CreateRotationZ ( rotationAngle ) * Matrix4.CreateTranslation ( 1, -1, -1 ), new Vector4 ( 1, 1, 1, 1 ) );

				// 스텐실 테스트 끔
				GL.Disable ( EnableCap.StencilTest );

				// 프레임버퍼 적용 해제 (백버퍼 적용)
				GL.BindFramebuffer ( FramebufferTarget.Framebuffer, 0 );
				
				// 백 버퍼에 대한 뷰포트 지정
				GL.Viewport ( 0, 0, 800, 600 );

				// 2D 텍스쳐 켬
				GL.Enable ( EnableCap.Texture2D );

				// 프레임버퍼 텍스쳐 입력
				GL.BindTexture ( TextureTarget.Texture2D, texture );

				// 프레임버퍼를 화면에 출력
				DrawFrameBuffer ( programId2, vertexBuffer2 );

				// 백 버퍼와 화면 버퍼 교환
				window.SwapBuffers ();
			};
			// 창이 종료될 때
			window.Closing += ( sender, e ) =>
			{
				GL.DeleteFramebuffer ( frameBuffer );
				GL.DeleteTexture ( texture );

				GL.DeleteProgram ( programId2 );
				GL.DeleteShader ( fragmentShader2 );
				GL.DeleteShader ( vertexShader2 );

				GL.DeleteProgram ( programId );
				GL.DeleteShader ( fragmentShader );
				GL.DeleteShader ( vertexShader );

				// 버퍼 제거
				//> 이 과정을 처리하지 않으면 비디오 메모리 누수가 발생할 수 있음
				GL.DeleteBuffer ( vertexBuffer );
				GL.DeleteBuffer ( vertexBuffer2 );
			};

			// 창을 띄우고 창이 종료될 때까지 메시지 펌프 및 무한 루프 처리
			window.Run ();
		}

		private static void DrawObject ( int programId, int vertexBuffer, int vertices, Matrix4 worldMatrix, Vector4 objectColor )
		{
			// 쉐이더 프로그램 사용
			GL.UseProgram ( programId );

			// 정점 버퍼 입력
			GL.BindBuffer ( BufferTarget.ArrayBuffer, vertexBuffer );

			// 정점 0번 입력 사용
			GL.EnableVertexAttribArray ( 0 );
			// 정점 0번은 float 3개 크기이며, 위치는 0이고 단위벡터가 아님
			GL.VertexAttribPointer ( 0, 3, VertexAttribPointerType.Float, false, sizeof ( float ) * 3, 0 );

			GL.Uniform4 ( GL.GetUniformLocation ( programId, "outColor" ), objectColor );

			// 월드 행렬 입력
			// 월드 변환 = 물체에 대한 변환
			GL.UniformMatrix4 ( GL.GetUniformLocation ( programId, "worldMatrix" ), false, ref worldMatrix );

			// 뷰 행렬 입력
			// 뷰 변환 = 카메라
			Matrix4 viewMatrix = Matrix4.LookAt (
				/* 카메라의 위치 */ new Vector3 ( 3, 2, 3 ),
				/* 카메라가 보고 있는 위치 */ new Vector3 (),
				/* 카메라의 상방 벡터 */ new Vector3 ( 0, 1, 0 )
				);
			GL.UniformMatrix4 ( GL.GetUniformLocation ( programId, "viewMatrix" ), false, ref viewMatrix );

			// 프로젝션 행렬 입력
			// 프로젝션 변환 = 3D 공간 좌표를 2D 공간 좌표로
			// Perspective Projection = 원근 투영
			// Orthographic Projection = 직교 투영
			Matrix4 projectionMatrix = Matrix4.CreatePerspectiveFieldOfView (
				/* 45도 각도로 내려봄 */ 3.141592f / 4,
				/* 화면 종횡비 */ 800 / 600.0f,
				/* 최소 시야 */ 0.001f,
				/* 최대 시야 */ 1000.0f
				);
			GL.UniformMatrix4 ( GL.GetUniformLocation ( programId, "projectionMatrix" ), false, ref projectionMatrix );

			// 정점 그리기
			GL.DrawArrays ( PrimitiveType.Triangles, 0, vertices );
		}

		private static void DrawFrameBuffer ( int programId, int vertexBuffer )
		{
			// 쉐이더 프로그램 사용
			GL.UseProgram ( programId );

			// 정점 버퍼 입력
			GL.BindBuffer ( BufferTarget.ArrayBuffer, vertexBuffer );

			// 정점 0, 1번 입력 사용
			GL.EnableVertexAttribArray ( 0 );
			GL.EnableVertexAttribArray ( 1 );
			// 정점 0번은 float 2개 크기이며, 위치는 0이고 단위벡터가 아님
			GL.VertexAttribPointer ( 0, 2, VertexAttribPointerType.Float, false, sizeof ( float ) * 4, 0 );
			// 정점 1번은 float 2개 크기이며, 위치는 8이고 단위벡터가 아님
			GL.VertexAttribPointer ( 1, 2, VertexAttribPointerType.Float, false, sizeof ( float ) * 4, 8 );

			GL.Uniform1 ( GL.GetUniformLocation ( programId, "fb" ), 0 );

			// 프로젝션 행렬 입력
			// 프로젝션 변환 = 3D 공간 좌표를 2D 공간 좌표로
			// Perspective Projection = 원근 투영
			// Orthographic Projection = 직교 투영
			Matrix4 projectionMatrix = Matrix4.CreateOrthographicOffCenter (
				/* 최좌단 */ 0,
				/* 최우단 */ 800,
				/* 최하단 */ 600,
				/* 최상단 */ 0,
				/* 최소 시야 */ 0.000f,
				/* 최대 시야 */ 1000.0f
				);
			GL.UniformMatrix4 ( GL.GetUniformLocation ( programId, "projectionMatrix" ), false, ref projectionMatrix );

			// 정점 그리기
			GL.DrawArrays ( PrimitiveType.Triangles, 0, 6 );

			GL.DisableVertexAttribArray ( 1 );
		}
	}
}
