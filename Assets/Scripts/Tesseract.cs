using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tesseract : MonoBehaviour {
	
	public Vector3 outerCubeSize;
	public Vector3 innerCubeSize;
	public Vector3 innerCubePosition;
	public Material material;
	
    protected Vector3[] newVertices;
    protected Vector2[] newUV;
    protected int[] newTriangles;
	
	uint[] sides = new uint[16] { 0xc3, 0x3c, 0x10099, 0x66, 0x9009, 0x1C003 , 0x1300C, 0x16006, 0x0990, 0x10C30, 0x103C0, 0x10660, 0xc300, 0x3c00, 0x19900, 0x6600 };
	
    void Start() {
		
		gameObject.AddComponent<MeshFilter>();
		gameObject.AddComponent<MeshRenderer>().material = this.material;
		
		GenerateMesh();
	}
	
	void Update(){
		GenerateMesh();
	}
	
	protected void GenerateMesh(){
		
		Vector4[] points = new Vector4[1] { new Vector4() };
		
		int n = 0;
		int i ;
		
		while( n < 4 ){
        	points = Reflect( points, 0, 1, n );
        	n++;
		}
				
		Vector3[] vertexList = new Vector3[ points.Length ];
		for( i=0; i<points.Length; i++ ) {
			Vector3 pos = new Vector3();
			pos.x = VertexPosition( points[i].x, points[i].w, outerCubeSize.x * -0.5f, outerCubeSize.x * 0.5f, innerCubePosition.x + innerCubeSize.x * -0.5f, innerCubePosition.x + innerCubeSize.x * 0.5f );
			pos.y = VertexPosition( points[i].y, points[i].w, outerCubeSize.y * -0.5f, outerCubeSize.y * 0.5f, innerCubePosition.y + innerCubeSize.y * -0.5f, innerCubePosition.y + innerCubeSize.y * 0.5f );
			pos.z = VertexPosition( points[i].z, points[i].w, outerCubeSize.z * -0.5f, outerCubeSize.z * 0.5f, innerCubePosition.z + innerCubeSize.z * -0.5f, innerCubePosition.z + innerCubeSize.z * 0.5f );
			vertexList[i] = pos;
		}
		
		newTriangles = new int[ sides.Length * 2 * 3 ];
		int[] newQuads = new int[ sides.Length * 4 ];
		newVertices = new Vector3[ sides.Length * 4 ];
		newUV = new Vector2[ newVertices.Length ];
		
		for( i=0; i<sides.Length; i++ ) {
			SideToQuad( sides[i] ).CopyTo( newQuads, i*4 );
			QuadToTri( new int[4]{4*i,4*i+1,4*i+2,4*i+3 } ).CopyTo( newTriangles, i*6 );
		}
		
		for( i=0; i < newQuads.Length; i++ ) {
			newVertices[i] = vertexList[ newQuads[i] ];
		}
		
		for( i=0; (i+2) < newVertices.Length; i+=3 ) {
			newUV[i] = new Vector2(0,0);
			newUV[i+1] = new Vector2(1,0);
			newUV[i+2] = new Vector2(0,1);
		}
				
        Mesh mesh = new Mesh();
		
        GetComponent<MeshFilter>().mesh = mesh;
		mesh.Clear();
        mesh.vertices = newVertices;
        mesh.uv = newUV;
        mesh.triangles = newTriangles;
		mesh.normals = new Vector3[newVertices.Length];
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		
	}

	Vector4[] Reflect(Vector4[] a, float v1, float v2, int n) {
        Vector4[] a1 = new Vector4[ a.Length ];
        Vector4[] a2 = new Vector4[ a.Length ];
		
		a.CopyTo( a1, 0 );
		a.CopyTo( a2, 0 );
		
        for( var i=0; i<a.Length; i++ ){
                a1[i][n] = v1;
                a2[i][n] = v2;
        }
		
		Vector4[].Reverse( a2 );
		
		a = new Vector4[ 2 * a.Length ];
		a1.CopyTo( a, 0 );
		a2.CopyTo( a, a1.Length );
        return a;
	}
	
	float VertexPosition(float x,float y, float l1, float r1, float l2, float r2) {
		var a=x*y;
		var b=x*(1f-y);
		var c=(1f-x)*y;
		var d=(1f-x)*(1f-y);
		
		return a*r2 + b*r1 + c*l2 + d*l1;
	}
	
	/*
	float VertexPosition(float a, float b) {
		return ( a - 0.5f ) * ( - 0.5f * b + 1f ) + 0.5f;
	}
	*/
	
	int[] SideToQuad( uint side ) {
		
		int n = 0;
		int vertexIndex = 0;
		int[] quad = new int[4];
		bool isReverseNormal = ( side & 0x10000 ) > 0;
		side = side & 0xffff;
		
		while( side > 0 ){
			if( ( side & 1 ) == 1 ){
				quad[vertexIndex] = n;
				vertexIndex++;
			}
			n++;
			side = side >> 1;
		}
		
		if( isReverseNormal ) {
			List<int> list = new List<int>( quad );
			list.Reverse();
			quad = list.ToArray();
		}
		return quad;
	}
	
	int[] QuadToTri( int[] quad ) {
		return new int[]{ quad[0], quad[1], quad[2], quad[2], quad[3], quad[0] };
	}

}
