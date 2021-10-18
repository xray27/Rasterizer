#version 330
//Source: https://learnopengl.com/Lighting/Basic-Lighting
//Soucre: https://learnopengl.com/Lighting/Multiple-lights
//Soucre: lecture 9

// shader input
in vec4 normal;			    // interpolated normal
in vec2 uv;			        // interpolated texture coordinates
in vec3 fragPos;            // the position of this fragment in world space

struct Light{
    vec3 position;          // position of the light
    vec3 color;             // color of the light
    vec3 specularColor;     // specular color of the light
    float strength;         // the strength of the light, the higher this number, the further the light will reach
    bool isSpot;            // wheter this light is a spotlight
    float angle;            // the cos of the angle of the spotlights cone
    vec3 dir;               // the direction of the spotlight
};
const int lightAmount = 12;
uniform vec3 ambient;                   // ambient light color
uniform Light lights[lightAmount];      // all lights

uniform vec3 camPos;		// camera position in world space

uniform float Kd;           // mesh diffuse coefficient. Diffuse color will be taken from texture, but multiplied by this coefficient
uniform vec3 Ks;            // mesh specular color
uniform vec3 Ka;            // mesh abient color
uniform int glosiness;      // mesh specular glosiness 

uniform sampler2D pixels;	// texture sampler

// shader output
out vec4 outputColor;

//Calculate the light color on fragment
vec3 CalculateLight(Light light, vec3 viewDir, vec3 normal, vec3 fragPos){
    //calculate some stuff
    vec3 lightDir = normalize(light.position - fragPos);            // direction from fragPos to the light position
    float dist = length(light.position - fragPos);                  // distance from fragPos to light
    vec3 reflectDir = normalize(reflect(-lightDir, normal));        // reflection direction
    float attenuation = 1.0f / ( (1.0f/light.strength) * dist * dist);
    
    // only add the light color for spotlights if fragPos inside spotlight cone
    if(light.isSpot && dot(lightDir, normalize(-light.dir)) < light.angle)
        return vec3(0,0,0);

    // diffuse reflection
    vec3 diffuse = max(dot(normal, lightDir), 0.0) * light.color * Kd * attenuation;

    // glossy reflection
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), glosiness);

    vec3 specular = spec * light.specularColor * Ks * attenuation;


    //return result
    return (diffuse + specular);
}

// fragment shader
void main()
{
    vec3 viewDir = normalize(camPos - fragPos);                     // viewing directionssssssssssssssssssssssssssssssswdqee
    vec3 materialColor = texture( pixels, uv ).xyz;
    vec3 color = materialColor * ambient * Ka;                      //calculate ambient color

    //Now add the color of all lights
    for(int i = 0; i < lightAmount; i++){
        color += materialColor * CalculateLight(lights[i], viewDir, normal.xyz, fragPos);
    }

    outputColor = vec4(color,1);
}


