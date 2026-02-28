<template>
  <h2>Manager Approval</h2>

  <table>
    <tr v-for="r in requests" :key="r.id">
      <td>{{ r.reason }}</td>
      <td>
        <button @click="approve(r.id)">Approve</button>
      </td>
    </tr>
  </table>
</template>

<script setup>
import { ref, onMounted } from "vue"
import api from "@/services/api"

const requests = ref([])

onMounted(async () => {
  const res = await api.get("/Leave/pending-manager")
  requests.value = res.data
})

const approve = async (id) => {
  await api.post(`/Leave/manager-approve/${id}`)
  alert("Approved")
  location.reload()
}
</script>