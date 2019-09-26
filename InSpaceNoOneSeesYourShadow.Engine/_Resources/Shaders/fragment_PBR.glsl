#version 440
out vec4 FragColor;
in vec2 f_texcoord;
in vec3 v_pos;
in vec3 v_norm;

uniform sampler2D maintexture;
uniform samplerCube skybox;

uniform float metallic;
uniform float roughness;
uniform float ao;
uniform vec3 camPos;
uniform float reflectionStrength;
uniform float refraction;

struct PointLight {
    vec3 position;
    vec3 color;
};

struct DirLight {
    vec3 direction;
    vec3 color;
    float lightStrength;
};

struct SpotLight{
    vec3 position;
    vec3 direction;
    float cutOff;
    float outerCutOff;
    vec3 color; 
};
uniform PointLight pointLight;
uniform DirLight dirLight;
uniform SpotLight spotLight[2];

const float PI = 3.14159265359;

float DistributionGGX(vec3 N, vec3 H, float roughness);
float GeometrySchlickGGX(float NdotV, float roughness);
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness);
vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0);
vec3 calculatePointLight(vec3 albedo, vec3 N, vec3 V);
vec3 calculateDirLight(vec3 albedo, vec3 N, vec3 V);
vec3 calculateSpotLight(vec3 albedo, vec3 N, vec3 V, SpotLight light);
vec3 calculateReflection(vec3 N, vec3 I);
vec3 calculateRefraction(vec3 N, vec3 I);

void main()
{
vec2 flipped_texcoord = vec2(f_texcoord.x, 1.0 - f_texcoord.y);
    vec3 albedo  = vec3(texture(maintexture, flipped_texcoord));
    vec3 N = normalize(v_norm);
    vec3 V = normalize(camPos - v_pos);
    vec3 I = normalize(v_pos - camPos);

    vec3 color = vec3(0);
    color += calculatePointLight(albedo, N, V);
    color += calculateDirLight(albedo, N, V);
    for(int i = 0; i < 2; i++)
        color += calculateSpotLight(albedo, N, V, spotLight[i]);
    color += calculateReflection(N, I);
    color += calculateRefraction(N, I);
    
    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0/2.2)); 
    FragColor = vec4(color, 1.0);
}

vec3 calculateReflection(vec3 N, vec3 I)
{
    
    vec3 R = reflect(I, N);
    vec3 skyboxReflect = texture(skybox, R).rgb * reflectionStrength;
    return skyboxReflect;
}

vec3 calculateRefraction(vec3 N, vec3 I)
{
    float ratio = 1.00 / refraction;
    vec3 R = refract(I, N, ratio);
    vec3 skyboxRefract;
    if(refraction != 0.0f)
        skyboxRefract = texture(skybox, R).rgb;
    else
        skyboxRefract = vec3(0);
    return skyboxRefract;
}
vec3 calculateSpotLight(vec3 albedo, vec3 N, vec3 V, SpotLight light)
{
    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metallic);

    vec3 Lo = vec3(0.0);
    vec3 L = normalize(light.position - v_pos);
    vec3 H = normalize(V + L);
    
    float distance    = length(light.position - v_pos);
    float attenuation = 1.0 / (distance);
    vec3 radiance     = light.color * attenuation;        

    float NDF = DistributionGGX(N, H, roughness);
    float NdotV = max(dot(N,V),0);
    float G   = GeometrySchlickGGX(NdotV, roughness);             
    vec3 F    = fresnelSchlickRoughness(max(dot(H, V), 0.0), F0);       

    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - metallic;	  

    vec3 numerator    = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0);
    vec3 specular     = numerator / max(denominator, 0.001);  

    float NdotL = max(dot(N, L), 0.0);                
    Lo += (kD * albedo / PI + specular) * radiance * NdotL;  

    vec3 ambient = vec3(0.03) * albedo * ao;
    vec3 color = ambient + Lo;

    float theta = dot(L, normalize(-light.direction)); 
    float epsilon = (light.cutOff - light.outerCutOff);
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    color *= intensity;
	//color *= 10;
    return color;
}

vec3 calculatePointLight(vec3 albedo, vec3 N, vec3 V)
{
    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metallic);

    vec3 Lo = vec3(0.0);
    vec3 L = normalize(pointLight.position - v_pos);
    vec3 H = normalize(V + L);
    float distance    = length(pointLight.position - v_pos);
    float attenuation = 1.0 / (distance);
    vec3 radiance     = pointLight.color * attenuation;        

    float NDF = DistributionGGX(N, H, roughness);
    float NdotV = max(dot(N,V),0);
    float G   = GeometrySchlickGGX(NdotV, roughness);             
    vec3 F    = fresnelSchlickRoughness(max(dot(H, V), 0.0), F0);       

    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - metallic;	  

    vec3 numerator    = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0);
    vec3 specular     = numerator / max(denominator, 0.001);  

    float NdotL = max(dot(N, L), 0.0);                
    Lo += (kD * albedo / PI + specular) * radiance * NdotL;  

    vec3 ambient = vec3(0.03) * albedo * ao;
    vec3 color = ambient + Lo;
    return color;
}

vec3 calculateDirLight(vec3 albedo, vec3 N, vec3 V)
{
    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metallic);

    vec3 Lo = vec3(0.0);
    vec3 L = normalize(dirLight.direction);
    vec3 H = normalize(V + L);
    vec3 radiance     = dirLight.color * dirLight.lightStrength;        

    float NDF = DistributionGGX(N, H, roughness);
    float NdotV = max(dot(N,V),0);
    float G   = GeometrySchlickGGX(NdotV, roughness);             
	vec3 F    = fresnelSchlickRoughness(max(dot(H, V), 0.0), F0);       

    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - metallic;	  

    vec3 numerator    = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0);
    vec3 specular     = numerator / max(denominator, 0.001);  

    float NdotL = max(dot(N, L), 0.0);                
    Lo += (kD * albedo / PI + specular) * radiance * NdotL;  

    vec3 ambient = vec3(0.03) * albedo * ao;
    vec3 color = ambient + Lo;
    return color; 
}

vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float num   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return num / denom;
}

float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}
