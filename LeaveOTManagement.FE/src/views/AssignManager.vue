<template>

<div class="assign-container">

<h2>Assign Manager</h2>

<select v-model="userId">

<option disabled value="">Select Employee</option>

<option v-for="u in users" :key="u.id" :value="u.id">
{{ u.fullName }}
</option>

</select>


<select v-model="managerId">

<option disabled value="">Select Manager</option>

<option v-for="m in managers" :key="m.id" :value="m.id">
{{ m.fullName }}
</option>

</select>


<button @click="assignManager">Assign</button>

</div>

</template>


<script setup>

import { ref, onMounted } from "vue"
import api from "@/services/api"

const users = ref([])
const managers = ref([])

const userId = ref("")
const managerId = ref("")

const loadData = async () => {

const userRes = await api.get("/users")
users.value = userRes.data

const managerRes = await api.get("/users/managers")
managers.value = managerRes.data

}

onMounted(loadData)


const assignManager = async () => {

try{

await api.put(`/users/assign-manager?userId=${userId.value}&managerId=${managerId.value}`)

alert("Manager assigned successfully")

}catch(err){

console.error(err)
alert("Error assigning manager")

}

}

</script>


<style scoped>

.assign-container{
width:400px;
margin:auto;
background:white;
padding:30px;
border-radius:10px;
box-shadow:0 5px 15px rgba(0,0,0,0.1);
display:flex;
flex-direction:column;
gap:10px;
}

select{
padding:10px;
border:1px solid #ccc;
border-radius:5px;
}

button{
padding:10px;
background:#28a745;
color:white;
border:none;
border-radius:5px;
cursor:pointer;
}

button:hover{
background:#218838;
}

</style>