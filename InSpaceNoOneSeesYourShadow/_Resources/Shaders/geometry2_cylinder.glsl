		#version 440 core
layout(lines_adjacency) in;
layout(triangle_strip, max_vertices = 64) out;

in VertexData{
	vec3 color;
} gs_in[];

out vec3 fColor;

uniform int facesNumber; 


void generateCircle(vec4 position, float radius, int n)
{
float pi = 3.14;
fColor = gs_in[0].color; // gs_in[0] since there's only one input vertex
for (int i = 0; i<n; i=i+2) {
	gl_Position = position; 
	EmitVertex();
	gl_Position = position + vec4(radius * sin(2*pi * i /n), radius * cos(2*pi * i /n), 0.0, 0.0);  
	EmitVertex();
	gl_Position = position + vec4(radius * sin(2*pi * (i+1)/n), radius * cos(2*pi * (i+1) /n), 0.0, 0.0); // 1:bottom-left   
	EmitVertex();

}
	gl_Position = position; 
	EmitVertex();
	gl_Position = position + vec4(0.5 * sin(0), radius * cos(0), 0.0, 0.0);  
	EmitVertex();
EndPrimitive();
}

void generateFaces(vec4 position1, vec4 position2, float radius, int n)
{
float pi = 3.14;
fColor = gs_in[0].color; // gs_in[0] since there's only one input vertex
for (int i = 0; i<n; i++) {
	gl_Position = position1 + vec4(radius * sin(2*pi * i /n), radius * cos(2*pi * i /n), 0.0, 0.0);  
	EmitVertex();
	gl_Position = position2 + vec4(radius * sin(2*pi * i /n), radius * cos(2*pi * i /n), 0.0, 0.0);  
	EmitVertex();
}
	gl_Position = position1 + vec4(radius * sin(0), radius * cos(0), 0.0, 0.0);  
	EmitVertex();
	gl_Position = position2 + vec4(radius * sin(0), radius * cos(0), 0.0, 0.0);  
	EmitVertex();
EndPrimitive();
}

void main() {
int n = facesNumber;
float radius = distance(gl_in[0].gl_Position, gl_in[1].gl_Position);
	generateCircle(gl_in[0].gl_Position, radius, n);
	generateCircle(gl_in[3].gl_Position, radius, n);
	generateFaces(gl_in[0].gl_Position, gl_in[3].gl_Position, radius, n);
};