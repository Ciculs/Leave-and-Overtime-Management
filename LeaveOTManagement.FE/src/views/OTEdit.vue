<template>
<div>

<h2>Edit OT</h2>

<form @submit.prevent="update">

<input type="date" v-model="form.date"/>
<input type="number" v-model="form.hours"/>
<textarea v-model="form.reason"/>

<button>Update</button>

</form>

</div>
</template>

<script setup>
import { reactive,onMounted } from "vue"
import { useRoute,useRouter } from "vue-router"
import api from "@/services/api"

const route=useRoute()
const router=useRouter()

const form=reactive({
date:"",
hours:1,
reason:""
})

onMounted(async()=>{
const res=await api.get(`/OT/${route.params.id}`)
Object.assign(form,res.data)
})

const update=async()=>{
await api.put(`/OT/${route.params.id}`,form)
alert("Updated")
router.push("/my-ot")
}
</script>