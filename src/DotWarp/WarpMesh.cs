using System;
using DotWarp.Effects;
using Nexus;
using SharpDX;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D10.Buffer;
using Device = SharpDX.Direct3D10.Device;

namespace DotWarp
{
	internal class WarpMesh : IDisposable
	{
		private readonly Device _device;
		private readonly Meshellator.Mesh _sourceMesh;
		private bool _initialized;
		private Buffer _vertexBuffer;
		private Buffer _indexBuffer;
		private Texture2D _texture;

		public bool IsOpaque
		{
			get { return _sourceMesh.Material.Transparency == 1.0f; }
		}

		public WarpMesh(Device device, Meshellator.Mesh sourceMesh)
		{
			_device = device;
			_sourceMesh = sourceMesh;
		}

		public void Initialize(ContentManager contentManager)
		{
			CreateVertexBuffer();
			CreateIndexBuffer();

			if (!string.IsNullOrEmpty(_sourceMesh.Material.DiffuseTextureName))
				_texture = contentManager.Load<Texture2D>(_sourceMesh.Material.DiffuseTextureName);

			_initialized = true;
		}

		private void CreateVertexBuffer()
		{
			DataStream vertexStream = new DataStream(
				_sourceMesh.Positions.Count * VertexPositionNormalTexture.SizeInBytes,
				false, true);
			for (int i = 0; i < _sourceMesh.Positions.Count; i++)
			{
				vertexStream.Write(_sourceMesh.Positions[i]);
				vertexStream.Write(_sourceMesh.Normals[i]);
				if (_sourceMesh.TextureCoordinates.Count - 1 >= i)
					vertexStream.Write(new Point2D(_sourceMesh.TextureCoordinates[i].X, _sourceMesh.TextureCoordinates[i].Y));
				else
					vertexStream.Write(Point2D.Zero);
			}
			vertexStream.Position = 0;
			_vertexBuffer = new Buffer(_device, vertexStream, (int)vertexStream.Length,
				ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None,
				ResourceOptionFlags.None);
			vertexStream.Dispose();
		}

		private void CreateIndexBuffer()
		{
			DataStream indexStream = new DataStream(_sourceMesh.Indices.Count * sizeof(short), false, true);
			for (int i = 0; i < _sourceMesh.Indices.Count; i++)
				indexStream.Write((short)_sourceMesh.Indices[i]);
			indexStream.Position = 0;
			_indexBuffer = new Buffer(_device, indexStream, (int)indexStream.Length,
				ResourceUsage.Default, BindFlags.IndexBuffer, CpuAccessFlags.None,
				ResourceOptionFlags.None);
			indexStream.Dispose();
		}

		public void Draw(BasicEffect effect)
		{
			if (!_initialized)
				throw new InvalidOperationException("Initialize must be called before Draw");

			_device.InputAssembler.SetVertexBuffers(0,
				new VertexBufferBinding(_vertexBuffer, VertexPositionNormalTexture.SizeInBytes, 0));
			_device.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R16_UInt, 0);

			effect.World = _sourceMesh.Transform.Value;
			effect.DiffuseColor = _sourceMesh.Material.DiffuseColor;
			effect.SpecularColor = _sourceMesh.Material.SpecularColor;
			effect.SpecularPower = _sourceMesh.Material.Shininess;

			if (!string.IsNullOrEmpty(_sourceMesh.Material.DiffuseTextureName))
			{
				effect.TextureEnabled = true;
				effect.Texture = _texture;
			}
			else
			{
				effect.TextureEnabled = false;
				effect.Texture = null;
			}

			effect.Alpha = _sourceMesh.Material.Transparency;

			effect.Begin();
			effect.CurrentTechnique.GetPassByIndex(0).Apply();

			_device.DrawIndexed(_sourceMesh.Indices.Count, 0, 0);

			_device.InputAssembler.SetIndexBuffer(null, Format.R16_UInt, 0);
			_device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding());
		}

		public void Dispose()
		{
			if (_vertexBuffer != null)
				_vertexBuffer.Dispose();
			if (_indexBuffer != null)
				_indexBuffer.Dispose();
		}
	}
}