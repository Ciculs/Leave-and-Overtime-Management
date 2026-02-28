<template>
  <h2>Create Leave</h2>

  <input type="date" v-model="date" />
  <input v-model="reason" placeholder="Reason" />

  <button @click="submit">Submit</button>
</template>

<script setup>
import { ref } from "vue"
import api from "@/services/api"

const date = ref("")
const reason = ref("")

const submit = async () => {
  await api.post("/Leave", {
    date: date.value,
    reason: reason.value
  })
  alert("Submitted")
}
</script>