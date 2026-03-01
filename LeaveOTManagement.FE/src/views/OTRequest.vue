<template>
  <div class="card">
    <h2>Register OT</h2>

    <form @submit.prevent="submit">
      <div class="row">
        <div class="field">
          <label>Date</label>
          <input type="date" v-model="form.date" required />
        </div>

        <div class="field">
          <label>From</label>
          <input type="time" v-model="form.fromTime" required />
        </div>

        <div class="field">
          <label>To</label>
          <input type="time" v-model="form.toTime" required />
        </div>
      </div>

      <div class="field">
        <label>Reason</label>
        <textarea v-model="form.reason" required />
      </div>

      <button class="primary">Submit</button>
    </form>
  </div>
</template>

<script setup>
import { reactive } from "vue"
import api from "@/services/api"

const emit = defineEmits(["success"])

const form = reactive({
  date: "",
  fromTime: "",
  toTime: "",
  reason: ""
})

const submit = async () => {
  try {

    if (form.toTime <= form.fromTime) {
      alert("Invalid time range")
      return
    }

    const payload = {
      reason: form.reason,
      details: [{
        workDate: new Date(form.date).toISOString(),
        fromTime: form.fromTime + ":00",
        toTime: form.toTime + ":00"
      }]
    }

    await api.post("/OT", payload)

    emit("success")

  } catch (err) {
    console.error(err)
    alert("Submit failed")
  }
}
</script>

<style scoped>
.card {
  background: white;
  padding: 30px;
  border-radius: 18px;
  box-shadow: 0 15px 40px rgba(0,0,0,.08);
}

.row {
  display: flex;
  gap: 15px;
}

.field {
  flex: 1;
  display: flex;
  flex-direction: column;
  margin-bottom: 18px;
}

input, textarea {
  padding: 10px;
  border-radius: 10px;
  border: 1px solid #ddd;
}

.primary {
  background: #5b2cff;
  color: white;
  border: none;
  padding: 12px;
  border-radius: 12px;
  cursor: pointer;
}
</style>