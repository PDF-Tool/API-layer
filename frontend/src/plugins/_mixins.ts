const parseResponse = function (response: any) {
  if (response.data.status === 'fail') {
    if (response.data.message) {
      let { message } = response.data

      if (response.data.errors) {
        message += '\n'

        for (const key in response.data.errors) {
          const keyErrors = response.data.errors[key]
          for (let i = 0; i < keyErrors.length; i++) {
            message += `\n- ${keyErrors[i]}`
          }
        }
      }

      console.error(message)
    }
    return Promise.reject(response)
  }
  return response
}

const parseErrorResponse = async function (error: any) {
  return error.response
}

export { parseResponse, parseErrorResponse }
