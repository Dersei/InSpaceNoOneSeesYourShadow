#version 440

layout(triangles) in;
layout(triangle_strip, max_vertices = 32) out;

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

vec3 createPerp(vec3 p1, vec3 p2)
{
  vec3 invec = normalize(p2 - p1);
  vec3 ret = cross( invec, vec3(0.0, 0.0, 1.0) );
  if ( length(ret) == 0.0 )
  {
    ret = cross( invec, vec3(0.0, 1.0, 0.0) );
  }
  return ret;
}

void main()
{
	VertexOut.v_normG = VertexIn[0].v_normG;
	VertexOut.v_posG = VertexIn[0].v_posG;
	VertexOut.f_texcoordG = VertexIn[0].f_texcoordG;
   float r1 = gl_in[0].gl_Position.w;
   float r2 = gl_in[1].gl_Position.w;

   vec3 axis = gl_in[1].gl_Position.xyz -gl_in[0].gl_Position.xyz;

   vec3 perpx = createPerp( gl_in[1].gl_Position.xyz, gl_in[1].gl_Position.xyz );
   vec3 perpy = cross( normalize(axis), perpx );
   int segs = 16;
   for(int i=0; i<segs; i++) {
      float a = i/float(segs-1) * 2.0 * 3.14159;
      float ca = cos(a); float sa = sin(a);
      vec3 normal = vec3( ca*perpx.x + sa*perpy.x,
                     ca*perpx.y + sa*perpy.y,
                     ca*perpx.z + sa*perpy.z );

      vec3 p1 = gl_in[0].gl_Position.xyz + r1*normal;
      vec3 p2 = gl_in[1].gl_Position.xyz + r2*normal;
      
      gl_Position = vec4(p1, 1.0); EmitVertex();
      gl_Position = vec4(p2, 1.0); EmitVertex();       
   }
   EndPrimitive();   
}