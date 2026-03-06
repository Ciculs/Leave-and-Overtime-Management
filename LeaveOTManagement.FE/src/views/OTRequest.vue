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
import { reactive, ref, computed } from "vue"
import api from "@/services/api"

const emit = defineEmits(["success"])

const loading = ref(false)

const form = reactive({
  date: "",
  fromTime: "",
  toTime: "",
  reason: ""
})

/* OT HOURS PREVIEW */

const hours = computed(() => {

  if(!form.fromTime || !form.toTime) return 0

  const start = new Date(`2000-01-01T${form.fromTime}`)
  const end = new Date(`2000-01-01T${form.toTime}`)

  const diff = (end - start) / (1000*60*60)

  return diff > 0 ? diff.toFixed(2) : 0

})

/* SUBMIT */

const submit = async () => {

  try {

    if (form.toTime <= form.fromTime) {
      alert("Invalid time range")
      return
    }

    loading.value = true

    const payload = {

  reason: form.reason,

  details: [
    {
      workDate: form.date + "T00:00:00",
      fromTime: form.fromTime + ":00",
      toTime: form.toTime + ":00"
    }
  ]

}

    await api.post("/OT", payload)

    alert("OT request created")

    /* reset form */

    form.date = ""
    form.fromTime = ""
    form.toTime = ""
    form.reason = ""

    emit("success")

  } catch (err) {

    console.error(err)
    alert("Submit failed")

  } finally {

    loading.value = false

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