#version 440

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

in VertexData {
    vec3 v_normG;
    vec3 v_posG;
    vec2 f_texcoordG;
} VertexIn[3];
 
out VertexData {
    vec3 v_normG;
    vec3 v_posG;
    vec2 f_texcoordG;
} VertexOut;

uniform float time;
uniform mat4 modelview;
uniform int n;

float sdCylinder( vec3 p, vec3 c )
{
  return length(p.xz-c.xy)-c.z;
}

//void main() {
//	VertexOut.v_normG = VertexIn[0].v_normG;
//	VertexOut.v_posG = VertexIn[0].v_posG;
//	VertexOut.f_texcoordG = VertexIn[0].f_texcoordG;
//  for(int i = 0; i < 3; i++) {
//	float d = sdCylinder(gl_in[i].gl_Position.xyz, vec3(1,1,1));
//	if(d<0.001)
//	{
//		gl_Position = gl_in[i].gl_Position.xyzw;
//	}
//	else
//	{
//		gl_Position = gl_in[i].gl_Position.xyzw+d;
//	}
//    EmitVertex();
//  }
//  EndPrimitive();
//}

void generateCone()
{
			float pt[n];
            var nt = new List<Vector3>();
            float r1 = 0.5f;
            float r2 = 0.0f;
            float h = 3;
            float nPhi = 50 * multiplier;
            float Phi = 0;
            float dPhi = (float)(2 * Math.PI / (nPhi - 1));
            float Nx = r1 - r2;
            float Ny = h;
            float N = (float) Math.Sqrt(Nx * Nx + Ny * Ny);
            Nx /= N; Ny /= N;
            for (var i = 0; i < nPhi; i++)
            {
                float cosPhi = (float)Math.Cos(Phi);
                float sinPhi = (float)Math.Sin(Phi);
                float cosPhi2 = (float)Math.Cos(Phi + dPhi / 2);
                float sinPhi2 = (float)Math.Sin(Phi + dPhi / 2);
                pt.Add(new Vector3(-h / 2, cosPhi * r1, sinPhi * r1));   // points
                nt.Add(new Vector3(Nx, Ny * cosPhi, Ny * sinPhi));         // normals
                pt.Add(new Vector3(h / 2, cosPhi2 * r2, sinPhi2 * r2));  // points
                nt.Add(new Vector3(Nx, Ny * cosPhi2, Ny * sinPhi2));       // normals
                Phi += dPhi;
            }
}

void main(void)
{

    VertexOut.v_normG = VertexIn[0].v_normG;
	VertexOut.f_texcoordG = VertexIn[0].f_texcoordG;
	VertexOut.v_posG = VertexIn[0].v_posG;
	gl_Position = normalize(vec4(gl_in[0].gl_Position.xyzw));
	EmitVertex();
	VertexOut.v_normG = VertexIn[1].v_normG;
	VertexOut.f_texcoordG = VertexIn[1].f_texcoordG;
	VertexOut.v_posG = VertexIn[1].v_posG;
	gl_Position = normalize(vec4(gl_in[1].gl_Position.xyzw));
	EmitVertex();
	VertexOut.v_normG = VertexIn[2].v_normG;
	VertexOut.f_texcoordG = VertexIn[2].f_texcoordG;
	VertexOut.v_posG = VertexIn[2].v_posG;
	gl_Position = normalize(vec4(gl_in[2].gl_Position.xyzw));
	EmitVertex();
	EndPrimitive();
}
