#version 440
out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
    vec4 FragPosLightSpace;
} fs_in;

uniform sampler2D diffuseTexture;
uniform sampler2D shadowMap;

uniform vec3 light_position;
uniform vec3 camPos;

    float ShadowCalculation(vec4 fragPosLightSpace)
    {
        
        vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;

        projCoords = projCoords * 0.5 + 0.5;
        float closestDepth = texture(shadowMap, projCoords.xy).r; 
        float currentDepth = projCoords.z;
		float bias = max(0.05 * (1.0 - dot(fs_in.Normal, light_position)), 0.005);  
        float shadow = currentDepth - bias > closestDepth  ? 1.0 : 0.0;

        return shadow;
    }

    void main()
    {           
		vec2 flipped_texcoord = vec2(fs_in.TexCoords.x, 1.0 - fs_in.TexCoords.y);
        vec3 color = texture(diffuseTexture, flipped_texcoord).rgb;
        vec3 normal = normalize(fs_in.Normal);
        vec3 lightColor = vec3(1.0);
        // ambient
        vec3 ambient = 0.15 * color;
        // diffuse
        vec3 lightDir = normalize(light_position - fs_in.FragPos);
        float diff = max(dot(lightDir, normal), 0.0);
        vec3 diffuse = diff * lightColor;
        // specular
        vec3 viewDir = normalize(camPos - fs_in.FragPos);
        float spec = 0.0;
        vec3 halfwayDir = normalize(lightDir + viewDir);  
        spec = pow(max(dot(normal, halfwayDir), 0.0), 64.0);
        vec3 specular = spec * lightColor;    
        float shadow = ShadowCalculation(fs_in.FragPosLightSpace);       
        vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * color;    

        FragColor = vec4(lighting, 1.0);
    }