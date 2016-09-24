from collections import UserDict

class GetAttrDict(UserDict):
  def __getattr__(self, name):
    if name in self.data:
      return self.data[name]
    raise AttributeError('No dict entry %s in %s' % (name, self))
