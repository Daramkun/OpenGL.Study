using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OGL.Study.Day9
{
	static class Program
	{
		static void GetImageRawData ( int textureId )
		{
			Bitmap image = new Bitmap ( Assembly.GetEntryAssembly ().GetManifestResourceStream ( "OGL.Study.Day9.Sample.png" ) );
			var data = image.LockBits ( new Rectangle ( new Point (), image.Size ), ImageLockMode.ReadOnly,
				System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, ( int ) TextureMinFilter.Linear );
			GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, ( int ) TextureMagFilter.Linear );
			GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, ( int ) TextureWrapMode.Repeat );
			GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, ( int ) TextureWrapMode.Repeat );

			GL.TexImage2D ( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0 );

			image.UnlockBits ( data );
			image.Dispose ();
		}

		[STAThread]
		static void Main ()
		{
			// OpenGL 창
			GameWindow window = new GameWindow ();

			int vertexBuffer = 0;
			int vertexShader = 0, fragmentShader = 0, programId = 0;
			int textureId = 0;

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
					-0.5f, -0.5f, -0.5f, 0.0f, 0.0f,
					 0.5f, -0.5f, -0.5f, 1.0f, 0.0f,
					 0.5f,  0.5f, -0.5f, 1.0f, 1.0f,
					 0.5f,  0.5f, -0.5f, 1.0f, 1.0f,
					-0.5f,  0.5f, -0.5f, 0.0f, 1.0f,
					-0.5f, -0.5f, -0.5f, 0.0f, 0.0f,

					-0.5f, -0.5f,  0.5f, 0.0f, 0.0f,
					 0.5f, -0.5f,  0.5f, 1.0f, 0.0f,
					 0.5f,  0.5f,  0.5f, 1.0f, 1.0f,
					 0.5f,  0.5f,  0.5f, 1.0f, 1.0f,
					-0.5f,  0.5f,  0.5f, 0.0f, 1.0f,
					-0.5f, -0.5f,  0.5f, 0.0f, 0.0f,

					-0.5f,  0.5f,  0.5f, 1.0f, 0.0f,
					-0.5f,  0.5f, -0.5f, 1.0f, 1.0f,
					-0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
					-0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
					-0.5f, -0.5f,  0.5f, 0.0f, 0.0f,
					-0.5f,  0.5f,  0.5f, 1.0f, 0.0f,

					 0.5f,  0.5f,  0.5f, 1.0f, 0.0f,
					 0.5f,  0.5f, -0.5f, 1.0f, 1.0f,
					 0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
					 0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
					 0.5f, -0.5f,  0.5f, 0.0f, 0.0f,
					 0.5f,  0.5f,  0.5f, 1.0f, 0.0f,

					-0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
					 0.5f, -0.5f, -0.5f, 1.0f, 1.0f,
					 0.5f, -0.5f,  0.5f, 1.0f, 0.0f,
					 0.5f, -0.5f,  0.5f, 1.0f, 0.0f,
					-0.5f, -0.5f,  0.5f, 0.0f, 0.0f,
					-0.5f, -0.5f, -0.5f, 0.0f, 1.0f,

					-0.5f,  0.5f, -0.5f, 0.0f, 1.0f,
					 0.5f,  0.5f, -0.5f, 1.0f, 1.0f,
					 0.5f,  0.5f,  0.5f, 1.0f, 0.0f,
					 0.5f,  0.5f,  0.5f, 1.0f, 0.0f,
					-0.5f,  0.5f,  0.5f, 0.0f, 0.0f,
					-0.5f,  0.5f, -0.5f, 0.0f, 1.0f
				};
				GL.BufferData<float> ( BufferTarget.ArrayBuffer, new IntPtr ( vertices.Length * sizeof ( float ) ), vertices, BufferUsageHint.StaticDraw );

				// 쉐이더 생성
				vertexShader = GL.CreateShader ( ShaderType.VertexShader );
				fragmentShader = GL.CreateShader ( ShaderType.FragmentShader );

				// 컴파일 할 소스 입력
				//> GLSL 요구 버전은 OpenGL 3.2 (GLSL 1.5)
				GL.ShaderSource ( vertexShader, @"#version 150
// 정점 쉐이더 입력 인자는 3차원 위치 벡터 하나와 2차원 텍스쳐 좌표 벡터 하나
in vec3 in_pos;
in vec2 in_tex;

// 정점 쉐이더 출력 인자는 2차원 텍스쳐 좌표 벡터 하나
out vec2 out_tex;

// 변환 행렬을 가져옴
uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main () {
	// 정점 위치 설정
	//> vec3를 vec4로 변환한 이유는 아핀 공간(Affine space)에 맞추기 위해서
	gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4 ( in_pos, 1 );
	out_tex = in_tex;
}" );
				GL.ShaderSource ( fragmentShader, @"#version 150
in vec2 out_tex;

uniform sampler2D sampler;

void main () {
	// 색상은 텍스처 좌표에 해당하는 색상
	gl_FragColor = texture ( sampler, out_tex );
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

				// 2D 텍스처 켜기
				GL.Enable ( EnableCap.Texture2D );

				// 텍스처 버퍼 생성
				textureId = GL.GenTexture ();
				GL.BindTexture ( TextureTarget.Texture2D, textureId );

				// 텍스처에 데이터 입력
				GetImageRawData ( textureId );
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
				// 화면 초기화(색상 버퍼, 깊이 버퍼, 스텐실 버퍼에 처리)
				GL.Clear ( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit );

				// 쉐이더 프로그램 사용
				GL.UseProgram ( programId );

				// 깊이 테스트 켬
				//> 켜지 않고 실행하면 어떤 결과가 일어나는지 확인해보자
				GL.Enable ( EnableCap.DepthTest );

				// 정점 버퍼 입력
				GL.BindBuffer ( BufferTarget.ArrayBuffer, vertexBuffer );

				// 정점 0번, 1번 입력 사용
				GL.EnableVertexAttribArray ( 0 );
				GL.EnableVertexAttribArray ( 1 );
				// 정점 0번은 float 3개 크기이며, 위치는 0이고 단위벡터가 아님
				GL.VertexAttribPointer ( 0, 3, VertexAttribPointerType.Float, false, sizeof ( float ) * 5, 0 );
				// 정점 1번은 float 2개 크기이며, 위치는 12이고 단위벡터가 아님
				GL.VertexAttribPointer ( 1, 2, VertexAttribPointerType.Float, false, sizeof ( float ) * 5, 12 );

				// 텍스처 버퍼 입력
				GL.BindTexture ( TextureTarget.Texture2D, textureId );
				GL.Uniform1 ( GL.GetUniformLocation ( programId, "sampler" ), 0 );

				// 월드 행렬 입력
				// 월드 변환 = 물체에 대한 변환
				Matrix4 worldMatrix = Matrix4.CreateRotationY ( rotationAngle ) /* << Y 좌표축을 고정하여 rotationAngle만큼 회전 */;
				GL.UniformMatrix4 ( GL.GetUniformLocation ( programId, "worldMatrix" ), false, ref worldMatrix );

				// 뷰 행렬 입력
				// 뷰 변환 = 카메라
				Matrix4 viewMatrix = Matrix4.LookAt (
					/* 카메라의 위치 */ new Vector3 ( 2, 2, 2 ),
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
				GL.DrawArrays ( PrimitiveType.Triangles, 0, 36 );

				// 백 버퍼와 화면 버퍼 교환
				window.SwapBuffers ();
			};
			// 창이 종료될 때
			window.Closing += ( sender, e ) =>
			{
				GL.DeleteTexture ( textureId );

				GL.DeleteProgram ( programId );
				GL.DeleteShader ( fragmentShader );
				GL.DeleteShader ( vertexShader );

				// 버퍼 제거
				//> 이 과정을 처리하지 않으면 비디오 메모리 누수가 발생할 수 있음
				GL.DeleteBuffer ( vertexBuffer );
			};

			// 창을 띄우고 창이 종료될 때까지 메시지 펌프 및 무한 루프 처리
			window.Run ();
		}
	}
}
