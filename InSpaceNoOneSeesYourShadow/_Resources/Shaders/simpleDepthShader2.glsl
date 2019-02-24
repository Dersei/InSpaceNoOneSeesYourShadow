#version 440

in vec3 vPosition;
in vec3 vNormal;
in vec2 texcoord;

uniform mat4 modelview;
uniform mat4 view;
uniform mat4 model;
uniform mat4 lightSpaceMatrix;

out VS_OUT {
        vec3 FragPos;
        vec3 Normal;
        vec2 TexCoords;
        vec4 FragPosLightSpace;
    } vs_out;

void main()
{
	   vs_out.FragPos = (model * vec4(vPosition, 1.0)).xyz;
        vs_out.Normal = transpose(inverse(mat3(model))) * vNormal;
        vs_out.TexCoords = texcoord;
        vs_out.FragPosLightSpace = lightSpaceMatrix * vec4(vs_out.FragPos, 1.0);
        gl_Position = modelview * vec4(vPosition, 1.0);
}