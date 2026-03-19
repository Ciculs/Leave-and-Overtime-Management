<template>

    <div class="create-user-container">

        <h2>Create New Employee</h2>

        <div class="form">

            <input v-model="fullName" placeholder="Full Name" />

            <input v-model="email" placeholder="Email" />

            <input v-model="username" placeholder="Username" />

            <input v-model="password" type="password" placeholder="Password" />

            <select v-model="departmentId">
                <option disabled value="">Select Department</option>

                <option v-for="d in departments" :key="d.id" :value="d.id">
                    {{ d.name }}
                </option>

            </select>


            <select v-model="roleId">
                <option disabled value="">Select Role</option>

                <option v-for="r in roles" :key="r.id" :value="r.id">
                    {{ r.name }}
                </option>

            </select>


            <select v-model="managerId">
                <option value="">Select Manager</option>

                <option v-for="m in managers" :key="m.id" :value="m.id">
                    {{ m.fullName }}
                </option>

            </select>


            <button @click="createUser">Create User</button>

        </div>

    </div>

</template>


<script setup>

import { ref, onMounted } from "vue"
import api from "@/services/api"

const fullName = ref("")
const email = ref("")
const username = ref("")
const password = ref("")

const departmentId = ref("")
const roleId = ref("")
const managerId = ref("")

const departments = ref([])
const roles = ref([])
const managers = ref([])

const loadData = async () => {

    try {

        const depRes = await api.get("/users/departments")
        departments.value = depRes.data

        const roleRes = await api.get("/users/roles")
        roles.value = roleRes.data

        const managerRes = await api.get("/users/managers")
        managers.value = managerRes.data

    } catch (err) {

        console.error("Load data error", err)

    }

}

onMounted(loadData)


const createUser = async () => {

    try {

        const res = await api.post("/users", {

            fullName: fullName.value,
            email: email.value,
            username: username.value,
            password: password.value,
            departmentId: Number(departmentId.value),
            roleId: Number(roleId.value),
            managerId: managerId.value ? Number(managerId.value) : null

        })

        alert("User created: " + res.data.employeeCode)

        fullName.value = ""
        email.value = ""
        username.value = ""
        password.value = ""
        departmentId.value = ""
        roleId.value = ""
        managerId.value = ""

    } catch (err) {

        console.error(err)
        alert("Create user failed")

    }

}

</script>


<style scoped>
.create-user-container {
    width: 400px;
    margin: auto;
    background: white;
    padding: 30px;
    border-radius: 10px;
    box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

.form {
    display: flex;
    flex-direction: column;
    gap: 10px;
}

input,
select {
    padding: 10px;
    border: 1px solid #ccc;
    border-radius: 5px;
}

button {
    padding: 10px;
    background: #007bff;
    color: white;
    border: none;
    border-radius: 5px;
    cursor: pointer;
}

button:hover {
    background: #0056b3;
}
</style>