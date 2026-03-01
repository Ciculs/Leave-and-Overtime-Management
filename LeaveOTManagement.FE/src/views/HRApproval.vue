<template>
  <div class="container">
    <h2>HR Approval</h2>

    <div v-if="requests.length === 0">
      No pending requests
    </div>

    <div v-for="item in requests" :key="item.id" class="card">
      <p><strong>Employee:</strong> {{ item.employeeName }}</p>
      <p><strong>Reason:</strong> {{ item.reason }}</p>
      <p><strong>Status:</strong> {{ item.status }}</p>

      <button @click="approve(item.id)">Approve</button>
      <button @click="reject(item.id)">Reject</button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import api from "../services/api"

const requests = ref([])

const fetchData = async () => {
  const res = await api.get("/Leave/pending-hr")
  requests.value = res.data
}

const approve = async (id) => {
  await api.put(`/Leave/hr-approve/${id}`)
  fetchData()
}

const reject = async (id) => {
  await api.put(`/Leave/hr-reject/${id}`)
  fetchData()
}

onMounted(fetchData)
</script>

<style>
.container {
  padding: 20px;
}
.card {
  border: 1px solid #ccc;
  padding: 10px;
  margin-bottom: 10px;
}
button {
  margin-right: 10px;
}
</style>