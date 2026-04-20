shader_type spatial;

void vertex() {
    // 只做这件事：Z-up 法线 → Y-up 翻转
    // 不乘矩阵、不加复杂东西、绝对不报错
    NORMAL = vec3(NORMAL.x, NORMAL.z, -NORMAL.y);
	TANGENT = vec3(TANGENT.x, TANGENT.z, -TANGENT.y);
}

void fragment() {
}