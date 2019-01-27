#version 330

out vec4 fragColor;
uniform float time;

void main()
{
    fragColor = vec4(time, 1.0, 1.0, 1.0);
}
