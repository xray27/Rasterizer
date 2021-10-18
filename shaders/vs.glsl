#version 330

// shader input
in vec2 vUV;				// vertex uv coordinate
in vec3 vNormal;			// untransformed vertex normal
in vec3 vPosition;			// untransformed vertex position

uniform mat4 Tmodel;	    // transformation matrix from local space to world space
uniform mat4 TprojectView;	// transformation matrix from world space to clip space
uniform vec3 camPos;		// camera position in world space

// shader output
out vec4 normal;			// interpolated normal
out vec2 uv;				// interpolated texture coordinates
out vec3 fragPos;			// the position of this fragment in world space

// vertex shader
void main()
{
	// transform vertex using supplied matrix
	gl_Position = TprojectView * Tmodel * vec4(vPosition, 1.0);
	fragPos = vec3(Tmodel * vec4(vPosition, 1.0));

	// forward normal and uv coordinate; will be interpolated over triangle
	normal = normalize(Tmodel * vec4(vNormal, 0.0f));
	uv = vUV;
}